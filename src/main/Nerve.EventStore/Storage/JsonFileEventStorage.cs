// Copyright 2014 https://github.com/Kostassoid/Nerve.EventStore
//   
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
//  
//      http://www.apache.org/licenses/LICENSE-2.0 
//  
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

namespace Kostassoid.Nerve.EventStore.Storage
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Threading.Tasks;
	using Model;
	using Newtonsoft.Json;

	public class JsonFileEventStorage : IEventStorage
	{
		readonly DirectoryInfo _folder;
		readonly object _sync = new object();

		public JsonFileEventStorage(string folderName)
		{
			_folder = Directory.CreateDirectory(folderName);

			ClearStorageFolder(_folder);
		}

		void ClearStorageFolder(DirectoryInfo folder)
		{
			lock (_sync)
			{
				folder.Delete(true);
				folder.Create();
			}
		}

		public Task<IEnumerable<IDomainEvent>> Load(Type type, Guid id)
		{
			var path = GetFilePathFor(type.Name, id);
			string contents;

			lock (_sync)
			{
				if (!File.Exists(path))
				{
					return new List<IDomainEvent>();
				}
				contents = File.ReadAllText(path, Encoding.UTF8);
			}

			var settings = new JsonSerializerSettings
						   {
							   TypeNameHandling = TypeNameHandling.Auto
						   };
			return (IEnumerable<IDomainEvent>)JsonConvert.DeserializeObject(contents, typeof(IEnumerable<IDomainEvent>), settings);
		}

		public void Save(Type type, Guid id, IEnumerable<IDomainEvent> events)
		{
			var path = GetFilePathFor(type.Name, id);

			lock (_sync)
			{
				var settings = new JsonSerializerSettings
							   {
								   TypeNameHandling = TypeNameHandling.Auto,
								   Formatting = Formatting.Indented
							   };
				File.WriteAllText(path, JsonConvert.SerializeObject(events, settings));
			}
		}

		private string GetFilePathFor(string type, Guid id)
		{
			return Path.Combine(_folder.FullName, string.Format("{0}-{1}.json", type, id));
		}
	}
}