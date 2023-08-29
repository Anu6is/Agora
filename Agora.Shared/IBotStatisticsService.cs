namespace Agora.Shared
{
    public interface IBotStatisticsService
    {
        public int GetTotalGuilds();
        public int GetTotalMembers();
        public int GetShardState(int shardId);
    }
}
