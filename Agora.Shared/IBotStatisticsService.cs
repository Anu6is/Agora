namespace Agora.Shared
{
    public interface IBotStatisticsService
    {
        public int GetTotalGuilds();
        public int GetTotalMembers();
        public string GetShardState(int shardId);
    }
}
