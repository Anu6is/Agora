namespace Agora.Shared
{
    public interface IDiscordBotService
    {
        public int GetTotalGuilds();
        public int GetTotalMembers();
        public int GetShardState(int shardId);
        public IEnumerable<ulong> GetMutualGuilds(IEnumerable<ulong> userGuilds);
    }
}
