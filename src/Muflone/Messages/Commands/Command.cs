using System;
using Muflone.Core;

namespace Muflone.Messages.Commands
{
	[Serializable]
	public class Command : ICommand
	{
		public IDomainId AggregateId { get; set; }
		public Guid CommitId { get; }
		public DateTime CommitDate { get; }
		public CommandHeaders Headers { get; set; }

		protected Command(IDomainId aggregateId, Guid correlationId, string who = "anonymous")
		{
			Headers = new CommandHeaders()
			{
				Who = who,
				CorrelationId = correlationId,
				When = DateTime.UtcNow,
				AggregateType = GetType().Name
			};
			CommitId = Guid.NewGuid();
			CommitDate = DateTime.UtcNow;
			AggregateId = aggregateId;
		}

		protected Command(IDomainId aggregateId, string who = "anonymous")
			: this(aggregateId, Guid.NewGuid(), who)
		{

		}

		public string Who => Headers.Who;

	}
}
