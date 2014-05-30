namespace Kostassoid.Nerve.EventStore.Specs.Command
{
	using System;
	using System.Linq.Expressions;
	using System.Threading.Tasks;
	using Model;

	public class CommandHandler2
	{
		readonly IEventStore _store;

		public CommandHandler2(IEventStore store)
		{
			_store = store;
		}

		public void Handle(CreateUser command)
		{
			Console.WriteLine("Creating user {0}.", command.Name);

			var user = User.Create(command.Id, command.Name, command.Age);
			_store.Commit(user).Wait();
		}

		public void Handle(CelebrateUserBirthday command)
		{
			Action act = () =>
			{
				//Console.WriteLine("Celebrating user birthday.");

				var user = _store.Load<User>(command.Id).Result;
				user.Birthday();
				_store.Commit(user).Wait();
			};

			PerformWithRetry(act);
		}

		public void Handle(ChangeUserName command)
		{
			Action act = () =>
			{
				Console.WriteLine("Changing user name to {0}.", command.NewName);

				var user = _store.Load<User>(command.Id).Result;
				user.ChangeName(command.NewName);
				_store.Commit(user).Wait();
			};

			PerformWithRetry(act);
		}

		static void PerformWithRetry(Action action)
		{
			while (true)
			{
				try
				{
					action();
					return;
				}
				catch (AggregateException ex)
				{
					if (ex.InnerException is ConcurrencyException)
					{
						Console.WriteLine("Concurrency exception [{0}]. Retrying...", ex);
					}
					else
					{
						throw;
					}
				}
				catch (ConcurrencyException ex)
				{
					Console.WriteLine("Concurrency exception [{0}]. Retrying...", ex);
				}
			}
		}
	}
}