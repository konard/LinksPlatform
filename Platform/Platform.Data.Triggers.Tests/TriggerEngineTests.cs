using System;
using Xunit;

namespace Platform.Data.Triggers.Tests
{
    public class TriggerEngineTests
    {
        [Fact]
        public void TestBasicTriggerExecution()
        {
            var storage = new MockLinkStorage();
            var engine = new TriggerEngine(storage);

            bool triggered = false;
            var trigger = new MarkovTrigger(
                "test-trigger",
                "A", "relates-to", "B",
                TriggerOperation.Create,
                context => { triggered = true; }
            );

            engine.RegisterTrigger(trigger);

            // Execute triggers
            engine.ExecuteTriggers("A", "relates-to", "B", TriggerOperation.Create);

            Assert.True(triggered);
        }

        [Fact]
        public void TestWildcardMatching()
        {
            var storage = new MockLinkStorage();
            var engine = new TriggerEngine(storage);

            bool triggered = false;
            var trigger = new MarkovTrigger(
                "wildcard-trigger",
                PatternMatcher.Wildcard,
                "relates-to",
                PatternMatcher.Wildcard,
                TriggerOperation.Create,
                context => { triggered = true; }
            );

            engine.RegisterTrigger(trigger);

            // Execute triggers with any source and target
            engine.ExecuteTriggers("X", "relates-to", "Y", TriggerOperation.Create);

            Assert.True(triggered);
        }

        [Fact]
        public void TestTriggerCancellation()
        {
            var storage = new MockLinkStorage();
            var engine = new TriggerEngine(storage);

            var trigger = new MarkovTrigger(
                "cancel-trigger",
                "forbidden", PatternMatcher.Wildcard, PatternMatcher.Wildcard,
                TriggerOperation.Create,
                context => { context.Cancel = true; }
            );

            engine.RegisterTrigger(trigger);

            // Execute triggers
            bool result = engine.ExecuteTriggers("forbidden", "action", "target", TriggerOperation.Create);

            Assert.False(result); // Operation should be cancelled
        }

        [Fact]
        public void TestTerminalTrigger()
        {
            var storage = new MockLinkStorage();
            var engine = new TriggerEngine(storage);

            int executionCount = 0;

            // Terminal trigger
            var terminalTrigger = new MarkovTrigger(
                "terminal",
                "A", PatternMatcher.Wildcard, PatternMatcher.Wildcard,
                TriggerOperation.Create,
                context => { executionCount++; },
                priority: 10,
                isTerminal: true
            );

            // Non-terminal trigger (lower priority)
            var normalTrigger = new MarkovTrigger(
                "normal",
                "A", PatternMatcher.Wildcard, PatternMatcher.Wildcard,
                TriggerOperation.Create,
                context => { executionCount++; },
                priority: 5
            );

            engine.RegisterTrigger(terminalTrigger);
            engine.RegisterTrigger(normalTrigger);

            engine.ExecuteTriggers("A", "B", "C", TriggerOperation.Create);

            // Only terminal trigger should execute
            Assert.Equal(1, executionCount);
        }

        [Fact]
        public void TestMarkovStyleExecution()
        {
            var storage = new MockLinkStorage();
            var engine = new TriggerEngine(storage);

            // Rule: A -> B
            var rule1 = new MarkovTrigger(
                "rule1",
                "A", PatternMatcher.Wildcard, PatternMatcher.Wildcard,
                TriggerOperation.Any,
                context => { context.Source = "B"; }
            );

            // Rule: B -> C (terminal)
            var rule2 = new MarkovTrigger(
                "rule2",
                "B", PatternMatcher.Wildcard, PatternMatcher.Wildcard,
                TriggerOperation.Any,
                context => { context.Source = "C"; },
                isTerminal: true
            );

            engine.RegisterTrigger(rule1);
            engine.RegisterTrigger(rule2);

            var result = engine.ExecuteMarkovStyle("A", "X", "Y", TriggerOperation.Any);

            // After Markov execution: A -> B -> C
            Assert.Equal("C", result.Source);
        }

        [Fact]
        public void TestPriorityOrdering()
        {
            var storage = new MockLinkStorage();
            var engine = new TriggerEngine(storage);

            int executionOrder = 0;
            int firstTriggerOrder = 0;
            int secondTriggerOrder = 0;

            var lowPriority = new MarkovTrigger(
                "low",
                PatternMatcher.Wildcard, PatternMatcher.Wildcard, PatternMatcher.Wildcard,
                TriggerOperation.Create,
                context => { firstTriggerOrder = ++executionOrder; },
                priority: 1
            );

            var highPriority = new MarkovTrigger(
                "high",
                PatternMatcher.Wildcard, PatternMatcher.Wildcard, PatternMatcher.Wildcard,
                TriggerOperation.Create,
                context => { secondTriggerOrder = ++executionOrder; },
                priority: 10
            );

            engine.RegisterTrigger(lowPriority);
            engine.RegisterTrigger(highPriority);

            engine.ExecuteTriggers("A", "B", "C", TriggerOperation.Create);

            // High priority should execute first
            Assert.Equal(1, secondTriggerOrder);
            Assert.Equal(2, firstTriggerOrder);
        }
    }
}
