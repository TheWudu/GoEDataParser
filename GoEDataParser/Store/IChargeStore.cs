namespace Charging
{
    namespace Store
    {
        public interface IChargeStore
        {
            public Charge Insert(Charge charge);
            public Charge? FindBySessionId(string session_id);
            public List<Charge> ReadAll();
            public long Count();
        }
    }
}
