using System;
using scrapsy.Services;

namespace scrapsy.Stores
{
    public class NewEgg : BotStore
    {
        public NewEgg(DirectoryService directoryService) : base(directoryService)
        {
        }

        protected override void OnConfigure()
        {
            throw new NotImplementedException();
        }

        protected override void OnStartUp()
        {
            throw new NotImplementedException();
        }

        protected override void OnLogIn()
        {
            throw new NotImplementedException();
        }

        protected override void OnCheckForItems()
        {
            throw new NotImplementedException();
        }

        protected override void OnCheckOut()
        {
            throw new NotImplementedException();
        }

        protected override void OnShutDown()
        {
            throw new NotImplementedException();
        }

        protected override void OnCheckCart()
        {
            throw new NotImplementedException();
        }
    }
}