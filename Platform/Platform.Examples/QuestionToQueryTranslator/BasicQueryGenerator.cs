using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples.QuestionToQueryTranslator
{
    /// <summary>
    /// Basic implementation of query generator that creates multiple interpretations
    /// </summary>
    public class BasicQueryGenerator : IQueryGenerator
    {
        public IEnumerable<IQuery> GenerateQueries(IQuestion question)
        {
            if (question == null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            var queries = new List<IQuery>();

            // Generate queries based on question type
            switch (question.QuestionType.ToLower())
            {
                case "what":
                    queries.AddRange(GenerateWhatQueries(question));
                    break;
                case "who":
                    queries.AddRange(GenerateWhoQueries(question));
                    break;
                case "when":
                    queries.AddRange(GenerateWhenQueries(question));
                    break;
                case "where":
                    queries.AddRange(GenerateWhereQueries(question));
                    break;
                case "why":
                    queries.AddRange(GenerateWhyQueries(question));
                    break;
                case "how":
                    queries.AddRange(GenerateHowQueries(question));
                    break;
                default:
                    queries.AddRange(GenerateGenericQueries(question));
                    break;
            }

            // Sort by confidence (descending)
            return queries.OrderByDescending(q => q.Confidence);
        }

        private IEnumerable<IQuery> GenerateWhatQueries(IQuestion question)
        {
            var queries = new List<IQuery>();

            // Definition query
            queries.Add(new Query(
                question,
                $"Find(Definition, Subject={question.Subject})",
                0.8,
                $"Find the definition or description of '{question.Subject}'"
            ));

            // Property query
            if (question.Context.Any())
            {
                queries.Add(new Query(
                    question,
                    $"Find(Property, Subject={question.Subject}, Property={string.Join(",", question.Context)})",
                    0.7,
                    $"Find the property '{string.Join(", ", question.Context)}' of '{question.Subject}'"
                ));
            }

            // Identity query
            queries.Add(new Query(
                question,
                $"Find(Identity, Subject={question.Subject})",
                0.6,
                $"Identify what '{question.Subject}' is"
            ));

            return queries;
        }

        private IEnumerable<IQuery> GenerateWhoQueries(IQuestion question)
        {
            return new[]
            {
                new Query(
                    question,
                    $"Find(Person, Role={question.Subject}, Action={question.Predicate})",
                    0.9,
                    $"Find the person who {question.Predicate} {question.Subject}"
                ),
                new Query(
                    question,
                    $"Find(Agent, Subject={question.Subject})",
                    0.7,
                    $"Find the agent/actor related to '{question.Subject}'"
                )
            };
        }

        private IEnumerable<IQuery> GenerateWhenQueries(IQuestion question)
        {
            return new[]
            {
                new Query(
                    question,
                    $"Find(Time, Event={question.Subject}, Action={question.Predicate})",
                    0.9,
                    $"Find the time when {question.Subject} {question.Predicate}"
                ),
                new Query(
                    question,
                    $"Find(TemporalRelation, Subject={question.Subject})",
                    0.7,
                    $"Find temporal information about '{question.Subject}'"
                )
            };
        }

        private IEnumerable<IQuery> GenerateWhereQueries(IQuestion question)
        {
            return new[]
            {
                new Query(
                    question,
                    $"Find(Location, Subject={question.Subject}, Action={question.Predicate})",
                    0.9,
                    $"Find the location where {question.Subject} {question.Predicate}"
                ),
                new Query(
                    question,
                    $"Find(SpatialRelation, Subject={question.Subject})",
                    0.7,
                    $"Find spatial/location information about '{question.Subject}'"
                )
            };
        }

        private IEnumerable<IQuery> GenerateWhyQueries(IQuestion question)
        {
            return new[]
            {
                new Query(
                    question,
                    $"Find(Reason, Subject={question.Subject}, Action={question.Predicate})",
                    0.8,
                    $"Find the reason why {question.Subject} {question.Predicate}"
                ),
                new Query(
                    question,
                    $"Find(Cause, Effect={question.Subject})",
                    0.7,
                    $"Find the cause that led to '{question.Subject}'"
                ),
                new Query(
                    question,
                    $"Find(Purpose, Subject={question.Subject})",
                    0.6,
                    $"Find the purpose or goal of '{question.Subject}'"
                )
            };
        }

        private IEnumerable<IQuery> GenerateHowQueries(IQuestion question)
        {
            return new[]
            {
                new Query(
                    question,
                    $"Find(Method, Subject={question.Subject}, Goal={question.Predicate})",
                    0.9,
                    $"Find the method or process for '{question.Subject}' to {question.Predicate}"
                ),
                new Query(
                    question,
                    $"Find(Manner, Action={question.Subject})",
                    0.7,
                    $"Find the manner or way in which '{question.Subject}' occurs"
                )
            };
        }

        private IEnumerable<IQuery> GenerateGenericQueries(IQuestion question)
        {
            return new[]
            {
                new Query(
                    question,
                    $"Search(Subject={question.Subject}, Predicate={question.Predicate})",
                    0.5,
                    $"General search for information related to '{question.Subject}' and '{question.Predicate}'"
                )
            };
        }
    }
}
