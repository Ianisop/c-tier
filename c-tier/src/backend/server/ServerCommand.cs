
namespace c_tier.src.backend.server
{
    public abstract class ServerCommand
    {

        abstract public string prefix { get; set; }

        public ServerCommand()
        {

        }

        /// <summary>
        /// Main method that comamnds run
        /// </summary>
        public abstract void Execute(string str);
    }
}
