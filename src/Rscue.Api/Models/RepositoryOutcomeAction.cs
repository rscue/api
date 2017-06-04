namespace Rscue.Api.Models
{
    using System;

    public struct RepositoryOutcomeAction : IEquatable<RepositoryOutcomeAction>
    {
        private readonly RepositoryOutcome _outcome;
        private readonly RepositoryAction _action;

        public static RepositoryOutcomeAction OkNone = new RepositoryOutcomeAction(RepositoryOutcome.Ok, RepositoryAction.None);

        public static RepositoryOutcomeAction OkCreated = new RepositoryOutcomeAction(RepositoryOutcome.Ok, RepositoryAction.Created);

        public static RepositoryOutcomeAction OkUpdated = new RepositoryOutcomeAction(RepositoryOutcome.Ok, RepositoryAction.Updated);

        public static RepositoryOutcomeAction NotFoundNone = new RepositoryOutcomeAction(RepositoryOutcome.NotFound, RepositoryAction.None);

        public static RepositoryOutcomeAction ValidationErrorNone = new RepositoryOutcomeAction(RepositoryOutcome.ValidationError, RepositoryAction.None);

        public RepositoryOutcomeAction(RepositoryOutcome outcome, RepositoryAction action)
        {
            this._outcome = outcome;
            this._action = action;
        }

        public RepositoryOutcome Outcome {  get { return _outcome; } }

        public RepositoryAction Action { get { return _action; } }

        public bool Equals(RepositoryOutcomeAction other) => Outcome == other.Outcome && Action == other.Action;

        public override bool Equals(object obj) => obj is RepositoryOutcomeAction ? Equals((RepositoryOutcomeAction)obj) : false;

        public override int GetHashCode() => unchecked(_outcome.GetHashCode() + _action.GetHashCode());

        public static bool operator ==(RepositoryOutcomeAction lhs, RepositoryOutcomeAction rhs) => lhs.Equals(rhs);

        public static bool operator !=(RepositoryOutcomeAction lhs, RepositoryOutcomeAction rhs) => !(lhs == rhs);

        public override string ToString() => this._outcome.ToString() + this._action.ToString();
    }
}
