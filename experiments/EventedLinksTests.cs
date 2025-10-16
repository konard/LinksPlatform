using System;
using System.Linq;
using Platform.Data.EventedIO;
using Xunit;

namespace Platform.Data.EventedIO.Tests
{
    /// <summary>
    /// Unit tests for the Evented Links API implementation
    /// Tests cover operation queuing, transactions, event triggers, and validation
    /// </summary>
    public class EventedLinksTests
    {
        [Fact]
        public void CreateAsync_ShouldEnqueueOperation()
        {
            // Arrange
            var links = new LinksEvented<long>();

            // Act
            var linkId = links.CreateAsync(1, 2);

            // Assert
            var pendingOps = links.GetPendingOperations().ToList();
            Assert.Single(pendingOps);
            Assert.Equal(OperationType.Create, pendingOps[0].Type);
            Assert.Equal(1L, pendingOps[0].Source);
            Assert.Equal(2L, pendingOps[0].Target);
        }

        [Fact]
        public void ReadAsync_ShouldEnqueueOperation()
        {
            // Arrange
            var links = new LinksEvented<long>();

            // Act
            links.ReadAsync(100);

            // Assert
            var pendingOps = links.GetPendingOperations().ToList();
            Assert.Single(pendingOps);
            Assert.Equal(OperationType.Read, pendingOps[0].Type);
            Assert.Equal(100L, pendingOps[0].LinkId);
        }

        [Fact]
        public void UpdateAsync_ShouldEnqueueOperation()
        {
            // Arrange
            var links = new LinksEvented<long>();

            // Act
            links.UpdateAsync(100, 3, 4);

            // Assert
            var pendingOps = links.GetPendingOperations().ToList();
            Assert.Single(pendingOps);
            Assert.Equal(OperationType.Update, pendingOps[0].Type);
            Assert.Equal(100L, pendingOps[0].LinkId);
            Assert.Equal(3L, pendingOps[0].NewSource);
            Assert.Equal(4L, pendingOps[0].NewTarget);
        }

        [Fact]
        public void DeleteAsync_ShouldEnqueueOperation()
        {
            // Arrange
            var links = new LinksEvented<long>();

            // Act
            links.DeleteAsync(100);

            // Assert
            var pendingOps = links.GetPendingOperations().ToList();
            Assert.Single(pendingOps);
            Assert.Equal(OperationType.Delete, pendingOps[0].Type);
            Assert.Equal(100L, pendingOps[0].LinkId);
        }

        [Fact]
        public void BeforeCreate_EventShouldFire()
        {
            // Arrange
            var links = new LinksEvented<long>();
            bool eventFired = false;
            links.BeforeCreate += (sender, args) => { eventFired = true; };

            // Act
            links.CreateAsync(1, 2);
            links.ProcessQueue();

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public void AfterCreate_EventShouldFire()
        {
            // Arrange
            var links = new LinksEvented<long>();
            bool eventFired = false;
            links.AfterCreate += (sender, args) => { eventFired = true; };

            // Act
            links.CreateAsync(1, 2);
            links.ProcessQueue();

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public void BeforeCreate_CanCancelOperation()
        {
            // Arrange
            var links = new LinksEvented<long>();
            bool afterCreateFired = false;
            links.BeforeCreate += (sender, args) => { args.Cancel = true; };
            links.AfterCreate += (sender, args) => { afterCreateFired = true; };

            // Act
            links.CreateAsync(1, 2);
            links.ProcessQueue();

            // Assert - AfterCreate should not fire if operation was cancelled
            Assert.False(afterCreateFired);
        }

        [Fact]
        public void ValidateOperation_Create_RequiresSourceAndTarget()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var operation = new LinkOperation<long>
            {
                Type = OperationType.Create,
                Source = 1
                // Missing Target
            };

            // Act
            bool isValid = links.ValidateOperation(operation, out var error);

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("Source and Target", error);
        }

        [Fact]
        public void ValidateOperation_Read_RequiresLinkId()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var operation = new LinkOperation<long>
            {
                Type = OperationType.Read
                // Missing LinkId
            };

            // Act
            bool isValid = links.ValidateOperation(operation, out var error);

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("LinkId", error);
        }

        [Fact]
        public void ValidateOperation_Update_RequiresLinkIdAndNewValues()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var operation = new LinkOperation<long>
            {
                Type = OperationType.Update,
                LinkId = 100
                // Missing NewSource and NewTarget
            };

            // Act
            bool isValid = links.ValidateOperation(operation, out var error);

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
        }

        [Fact]
        public void ValidateOperation_Delete_RequiresLinkId()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var operation = new LinkOperation<long>
            {
                Type = OperationType.Delete
                // Missing LinkId
            };

            // Act
            bool isValid = links.ValidateOperation(operation, out var error);

            // Assert
            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("LinkId", error);
        }

        [Fact]
        public void CheckConflicts_UpdateAfterDelete_ShouldDetectConflict()
        {
            // Arrange
            var links = new LinksEvented<long>();
            links.DeleteAsync(100);

            var updateOp = new LinkOperation<long>
            {
                Type = OperationType.Update,
                LinkId = 100,
                NewSource = 5,
                NewTarget = 6
            };

            // Act
            bool hasConflict = links.CheckConflicts(updateOp, out var conflicts);

            // Assert
            Assert.True(hasConflict);
            Assert.NotEmpty(conflicts);
        }

        [Fact]
        public void BeginTransaction_ShouldCreateTransaction()
        {
            // Arrange
            var links = new LinksEvented<long>();

            // Act
            var transaction = links.BeginTransaction();

            // Assert
            Assert.NotNull(transaction);
            Assert.NotEqual(Guid.Empty, transaction.TransactionId);
            Assert.Empty(transaction.Operations);
            Assert.False(transaction.IsCommitted);
        }

        [Fact]
        public void CommitTransaction_WithValidOperations_ShouldSucceed()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var transaction = links.BeginTransaction();

            var createOp = new LinkOperation<long>
            {
                Type = OperationType.Create,
                Source = 1,
                Target = 2,
                LinkId = 100,
                TransactionId = transaction.TransactionId
            };
            transaction.Operations.Add(createOp);

            // Act
            bool result = links.CommitTransaction(transaction.TransactionId);

            // Assert
            Assert.True(result);
            Assert.True(transaction.IsCommitted);
        }

        [Fact]
        public void CommitTransaction_WithInconsistentOperations_ShouldThrow()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var transaction = links.BeginTransaction();

            // Try to update a link that doesn't exist
            var updateOp = new LinkOperation<long>
            {
                Type = OperationType.Update,
                LinkId = 999,
                NewSource = 1,
                NewTarget = 2,
                TransactionId = transaction.TransactionId
            };
            transaction.Operations.Add(updateOp);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => links.CommitTransaction(transaction.TransactionId));
        }

        [Fact]
        public void CheckConsistency_CreateThenUpdate_ShouldBeConsistent()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var operations = new[]
            {
                new LinkOperation<long>
                {
                    Type = OperationType.Create,
                    Source = 1,
                    Target = 2,
                    LinkId = 100
                },
                new LinkOperation<long>
                {
                    Type = OperationType.Update,
                    LinkId = 100,
                    NewSource = 3,
                    NewTarget = 4
                }
            };

            // Act
            bool isConsistent = links.CheckConsistency(operations, out var error);

            // Assert
            Assert.True(isConsistent);
            Assert.Null(error);
        }

        [Fact]
        public void CheckConsistency_UpdateWithoutCreate_ShouldBeInconsistent()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var operations = new[]
            {
                new LinkOperation<long>
                {
                    Type = OperationType.Update,
                    LinkId = 100,
                    NewSource = 3,
                    NewTarget = 4
                }
            };

            // Act
            bool isConsistent = links.CheckConsistency(operations, out var error);

            // Assert
            Assert.False(isConsistent);
            Assert.NotNull(error);
        }

        [Fact]
        public void CheckConsistency_DeleteThenUpdate_ShouldBeInconsistent()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var operations = new[]
            {
                new LinkOperation<long>
                {
                    Type = OperationType.Create,
                    Source = 1,
                    Target = 2,
                    LinkId = 100
                },
                new LinkOperation<long>
                {
                    Type = OperationType.Delete,
                    LinkId = 100
                },
                new LinkOperation<long>
                {
                    Type = OperationType.Update,
                    LinkId = 100,
                    NewSource = 3,
                    NewTarget = 4
                }
            };

            // Act
            bool isConsistent = links.CheckConsistency(operations, out var error);

            // Assert
            Assert.False(isConsistent);
            Assert.NotNull(error);
        }

        [Fact]
        public void RollbackTransaction_ShouldRemoveTransaction()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var transaction = links.BeginTransaction();
            var initialCount = links.GetPendingTransactions().Count();

            // Act
            links.RollbackTransaction(transaction.TransactionId);

            // Assert
            Assert.Equal(initialCount - 1, links.GetPendingTransactions().Count());
        }

        [Fact]
        public void ClearQueue_ShouldRemoveAllPendingOperations()
        {
            // Arrange
            var links = new LinksEvented<long>();
            links.CreateAsync(1, 2);
            links.CreateAsync(3, 4);
            links.UpdateAsync(100, 5, 6);

            // Act
            links.ClearQueue();

            // Assert
            Assert.Empty(links.GetPendingOperations());
        }

        [Fact]
        public void OnValidationError_ShouldFireForInvalidOperation()
        {
            // Arrange
            var links = new LinksEvented<long>();
            bool eventFired = false;
            string? errorMessage = null;

            links.OnValidationError += (sender, args) =>
            {
                eventFired = true;
                errorMessage = args.ValidationError;
                args.Cancel = true; // Prevent exception
            };

            var invalidOp = new LinkOperation<long>
            {
                Type = OperationType.Create,
                Source = 1
                // Missing Target
            };

            // Act
            links.EnqueueOperation(invalidOp);

            // Assert
            Assert.True(eventFired);
            Assert.NotNull(errorMessage);
        }

        [Fact]
        public void OnConflict_ShouldFireForConflictingOperations()
        {
            // Arrange
            var links = new LinksEvented<long>();
            bool eventFired = false;

            links.OnConflict += (sender, args) =>
            {
                eventFired = true;
                args.Cancel = true; // Prevent exception
            };

            // Queue a delete operation
            links.DeleteAsync(100);

            var conflictingOp = new LinkOperation<long>
            {
                Type = OperationType.Update,
                LinkId = 100,
                NewSource = 5,
                NewTarget = 6
            };

            // Act
            links.EnqueueOperation(conflictingOp);

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public void ProcessQueue_ShouldExecuteAllOperationsInOrder()
        {
            // Arrange
            var links = new LinksEvented<long>();
            var executionOrder = new System.Collections.Generic.List<OperationType>();

            links.AfterCreate += (sender, args) => executionOrder.Add(OperationType.Create);
            links.AfterUpdate += (sender, args) => executionOrder.Add(OperationType.Update);
            links.AfterDelete += (sender, args) => executionOrder.Add(OperationType.Delete);

            // Act
            links.CreateAsync(1, 2);
            links.UpdateAsync(default, 3, 4);
            links.DeleteAsync(100);
            links.ProcessQueue();

            // Assert
            Assert.Equal(3, executionOrder.Count);
            Assert.Equal(OperationType.Create, executionOrder[0]);
            Assert.Equal(OperationType.Update, executionOrder[1]);
            Assert.Equal(OperationType.Delete, executionOrder[2]);
        }

        [Fact]
        public void MultipleTransactions_ShouldBeTrackedIndependently()
        {
            // Arrange
            var links = new LinksEvented<long>();

            // Act
            var transaction1 = links.BeginTransaction();
            var transaction2 = links.BeginTransaction();

            // Assert
            Assert.NotEqual(transaction1.TransactionId, transaction2.TransactionId);
            Assert.Equal(2, links.GetPendingTransactions().Count());
        }
    }
}
