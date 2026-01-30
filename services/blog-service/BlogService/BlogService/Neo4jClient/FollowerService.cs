using BlogService.Models;
using Neo4j.Driver;

namespace BlogService.Neo4jClient;

public class FollowerService : IDisposable
{
    private readonly IDriver _driver;

    public FollowerService(Neo4jSettings settings)
    {
        _driver = GraphDatabase.Driver(settings.Uri, AuthTokens.Basic(settings.User, settings.Password));
    }

    // Follow a user
    public async Task<bool> FollowUser(string followerId, string followerUsername, string followeeId, string followeeUsername)
    {
        await using var session = _driver.AsyncSession();
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                // Create or merge follower node
                await tx.RunAsync(
                    "MERGE (follower:User {userId: $followerId, username: $followerUsername})",
                    new { followerId, followerUsername });

                // Create or merge followee node
                await tx.RunAsync(
                    "MERGE (followee:User {userId: $followeeId, username: $followeeUsername})",
                    new { followeeId, followeeUsername });

                // Create FOLLOWS relationship
                await tx.RunAsync(
                    @"MATCH (follower:User {userId: $followerId})
                      MATCH (followee:User {userId: $followeeId})
                      MERGE (follower)-[:FOLLOWS]->(followee)",
                    new { followerId, followeeId });
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error following user: {ex.Message}");
            return false;
        }
    }

    // Unfollow a user
    public async Task<bool> UnfollowUser(string followerId, string followeeId)
    {
        await using var session = _driver.AsyncSession();
        try
        {
            await session.ExecuteWriteAsync(async tx =>
            {
                await tx.RunAsync(
                    @"MATCH (follower:User {userId: $followerId})-[r:FOLLOWS]->(followee:User {userId: $followeeId})
                      DELETE r",
                    new { followerId, followeeId });
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unfollowing user: {ex.Message}");
            return false;
        }
    }

    // Check if user is following another user
    public async Task<bool> IsFollowing(string followerId, string followeeId)
    {
        await using var session = _driver.AsyncSession();
        try
        {
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (follower:User {userId: $followerId})-[:FOLLOWS]->(followee:User {userId: $followeeId})
                      RETURN count(*) as count",
                    new { followerId, followeeId });

                var record = await cursor.SingleAsync();
                return record["count"].As<int>() > 0;
            });
            return result;
        }
        catch
        {
            return false;
        }
    }

    // Get followers of a user
    public async Task<List<(string userId, string username)>> GetFollowers(string userId)
    {
        await using var session = _driver.AsyncSession();
        try
        {
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (follower:User)-[:FOLLOWS]->(user:User {userId: $userId})
                      RETURN follower.userId as userId, follower.username as username",
                    new { userId });

                var followers = new List<(string userId, string username)>();
                await foreach (var record in cursor)
                {
                    followers.Add((
                        record["userId"].As<string>(),
                        record["username"].As<string>()
                    ));
                }
                return followers;
            });
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting followers: {ex.Message}");
            return new List<(string, string)>();
        }
    }

    // Get users that this user is following
    public async Task<List<(string userId, string username)>> GetFollowing(string userId)
    {
        await using var session = _driver.AsyncSession();
        try
        {
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (user:User {userId: $userId})-[:FOLLOWS]->(followee:User)
                      RETURN followee.userId as userId, followee.username as username",
                    new { userId });

                var following = new List<(string userId, string username)>();
                await foreach (var record in cursor)
                {
                    following.Add((
                        record["userId"].As<string>(),
                        record["username"].As<string>()
                    ));
                }
                return following;
            });
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting following: {ex.Message}");
            return new List<(string, string)>();
        }
    }

    // Get follow recommendations (friends of friends)
    public async Task<List<(string userId, string username)>> GetRecommendations(string userId, int limit = 5)
    {
        await using var session = _driver.AsyncSession();
        try
        {
            var result = await session.ExecuteReadAsync(async tx =>
            {
                var cursor = await tx.RunAsync(
                    @"MATCH (user:User {userId: $userId})-[:FOLLOWS]->(friend:User)-[:FOLLOWS]->(recommendation:User)
                      WHERE NOT (user)-[:FOLLOWS]->(recommendation) AND recommendation.userId <> $userId
                      RETURN DISTINCT recommendation.userId as userId, recommendation.username as username
                      LIMIT $limit",
                    new { userId, limit });

                var recommendations = new List<(string userId, string username)>();
                await foreach (var record in cursor)
                {
                    recommendations.Add((
                        record["userId"].As<string>(),
                        record["username"].As<string>()
                    ));
                }
                return recommendations;
            });
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting recommendations: {ex.Message}");
            return new List<(string, string)>();
        }
    }

    public void Dispose()
    {
        _driver?.Dispose();
    }
}
