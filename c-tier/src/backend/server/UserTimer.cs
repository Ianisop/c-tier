using c_tier.src.backend.client;


namespace c_tier.src.backend.server
{
    public class UserTimer
    {
        public User user { get; set; }
        public System.Timers.Timer timer { get; set; }

        public UserTimer()
        {

        }
    }

}
