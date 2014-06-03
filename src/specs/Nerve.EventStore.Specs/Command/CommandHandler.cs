namespace Kostassoid.Nerve.EventStore.Specs.Command
{
	using System;
	using Model;

	public class CommandHandler
	{
		readonly IEventStore _store;

		public CommandHandler(IEventStore store)
		{
			_store = store;
		}

		public void Handle(CreateUser command)
		{
			var user = User.Create(command.Id, command.Name, command.Age);
			_store.Commit(user).Wait();
		}

		public void Handle(CelebrateUserBirthday command)
		{
			Action act = () =>
			{
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