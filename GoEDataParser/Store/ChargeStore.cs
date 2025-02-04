namespace Charging
{
    namespace Store
    {
        class ChargeStore : IChargeStore
        {
            readonly IChargeStore store;

            public ChargeStore()
            {
                store = new MongoStore();
            }

            public Charge? FindBySessionId(string session_id)
            {
                return store.FindBySessionId(session_id);
            }

            public Charge First()
            {
                return store.First();
            }

            public Charge Insert(Charge charge)
            {
                return store.Insert(charge);
            }

            public long Count()
            {
                return store.Count();
            }
        };
    }
}
