using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace RaftImplementation.Repository
{
    public class OrderEventRepository : IOrderEventRepository
    {
        string constr = "Data Source=IN01LAP80323\\DEV2016;Initial Catalog=test;Integrated Security=True";
       
        public async Task<string> GetStatus(Guid instanceId)
        {
            
            
            var status = string.Empty;
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
            
                var sqlQuery = $"SELECT STATUS FROM OrderEventScheduler WHERE InstanceId='{instanceId}'";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                status = (await sqlcmd.ExecuteScalarAsync()).ToString();
                sqlcmd.Dispose();

            }
            return status;
            
        }

        public async Task UpdateProcessorState(Guid instanceId, OrderEventProcessorState state)
        {
            
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"UPDATE OrderEventScheduler SET STATE='{Enum.GetName(typeof(OrderEventProcessorState),state)}', STATUS='NORMAL' WHERE InstanceId='{instanceId}'";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var result = await sqlcmd.ExecuteNonQueryAsync();
                sqlcmd.Dispose();
            }
        }

        public async Task DeclareElection()
        {
            
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"UPDATE OrderEventScheduler SET STATUS='ELECTION' WHERE ISDELETED=0";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                await sqlcmd.ExecuteNonQueryAsync();
                sqlcmd.Dispose();
            }
        }
        public async Task WithdrawElection(Guid instanceId)
        {

            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"UPDATE OrderEventScheduler SET STATUS='NORMAL' WHERE InstanceId='{instanceId}'";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                await sqlcmd.ExecuteNonQueryAsync();
                sqlcmd.Dispose();
            }
        }

        public Task UpdateOrderEvent(int eventId, OrderEventStatus orderEventStatus)
        {
            throw new NotImplementedException();
        }

        public async Task  CastVote(Guid candidateId)
        {
            
          
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"Update OrderEventScheduler set Vote=Vote+1 Where InstanceId='{candidateId}'";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var orderEventReader = await sqlcmd.ExecuteNonQueryAsync();                
                sqlcmd.Dispose();
            }
        }

        public Task<IList<int>> GetOrderEvent(Guid instanceId)
        {
            throw new NotImplementedException();
        }

       
        public async Task AssignOrderEventToFollower()
        {
            
            var instanceIds = new List<Guid>();
            var eventProcessorList = new Dictionary<int, int>();
            using (var sqlcon = new SqlConnection())
            {
                sqlcon.Open();
          //      var location = Environment.MachineName;
                var sqlQuery = $"SELECT InstanceId FROM OrderEventScheduler WHERE STATE='FOLLOWER' AND ISDELETED=0";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var instanceReader = await sqlcmd.ExecuteReaderAsync();
                while (await instanceReader.ReadAsync())
                {
                    instanceIds.Add(Guid.Parse(instanceReader["InstanceId"].ToString()));
                }
                instanceReader.Close();
                sqlcmd.Dispose();

                sqlQuery = $"SELECT top 10 EventId FROM OrderEventSchedulerOutbox WHERE Status = 'SUBMITTED'";
                sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var reader = await sqlcmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var eventId = Convert.ToInt32(reader["EventId"]);
                    eventProcessorList.Add(eventId, eventId % (instanceIds.Count + 1));

                }
                reader.Close();
                sqlcmd.Dispose();
                foreach (var eventInstance in eventProcessorList)
                {
                    sqlQuery = $"UPDATE OrderEventSchedulerOutbox SET InstanceId = {eventInstance.Value} WHERE EVENTID = {eventInstance.Key}";
                    sqlcmd = sqlcon.CreateCommand();
                    sqlcmd.CommandText = sqlQuery;
                    await sqlcmd.ExecuteNonQueryAsync();
                }
                sqlcmd.Dispose();
            }
        }

        public async Task UpdateHeartBeat(Guid Id)
        {
            
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var location = Environment.MachineName;               

                var sqlQuery = $"MERGE INTO OrderEventScheduler AS target";
                sqlQuery += $" USING(SELECT '{Id}') AS source(InstanceId)";
                sqlQuery += "   ON(target.InstanceId = source.InstanceId)";
                sqlQuery += "    WHEN MATCHED THEN";
                sqlQuery += "       UPDATE SET[HEARTBEATON] = GETDATE()";
                sqlQuery += "  WHEN NOT MATCHED THEN";
                sqlQuery += "     INSERT([InstanceId], StartedOn, [HEARTBEATON], [Vote], [Status], [State], iSDeleted)";
                 sqlQuery += $"    VALUES('{Id}', Getdate(), GetDate(), 0, 'NORMAL','Follower', 0);";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var result = await sqlcmd.ExecuteNonQueryAsync();
                sqlcmd.Dispose();
            }
        }

        public async Task<IList<Guid>> GetActiveCandidates()
        {
            
            var candidates = new List<Guid>();
            using (var sqlcon = new SqlConnection(constr))
            {
                var sqlQuery = $"SELECT InstanceId FROM OrderEventScheduler WHERE State='CANDIDATE' AND ISDELETED=0";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var sqlReader = await sqlcmd.ExecuteReaderAsync();
                while (await sqlReader.ReadAsync())
                {
                    candidates.Add(Guid.Parse(sqlReader[0].ToString()));
                }
                sqlcmd.Dispose();
            }

            return candidates;
        }

        public async Task<DateTime?> GetLeaderHeartBeat()
        {
            DateTime? leaderHeartBeatOn=null;
            
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"SELECT HEARTBEATON FROM OrderEventScheduler WHERE STATE='LEADER' AND ISDELETED=0";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var result = await sqlcmd.ExecuteScalarAsync();
                if(result !=null)
                    leaderHeartBeatOn = Convert.ToDateTime(result);
              
                    

                sqlcmd.Dispose();
            }
            return leaderHeartBeatOn;
        }

        public async Task<IList<Guid>> GetProcessors()
        {
            
            var processors = new List<Guid>();
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"SELECT InstanceId from OrderEventScheduler where IsDeleted=0 AND DATEDIFF(ss,HeartBeatOn, GETDATE()) <= 7";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var sqlReader = sqlcmd.ExecuteReaderAsync();
                while (await sqlReader.Result.ReadAsync())
                {
                    processors.Add(Guid.Parse(sqlReader.Result[0].ToString()));
                }
                sqlcmd.Dispose();
            }
            return processors;
         }

        public async Task<Guid> GetCandidateWithMaxVote() {
            Guid leaderId = Guid.Empty;
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"SELECT Top 1 InstanceId  FROM OrderEventScheduler WHERE ISDELETED=0 ORDER BY Vote DESC";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var result = await sqlcmd.ExecuteScalarAsync();
                if (result != null)
                    leaderId = Guid.Parse(result.ToString());
                sqlcmd.Dispose();
            }
            return leaderId;
        }

        public async Task RegisterOrderEventScheduler(Guid processorId)
        {
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = "INSERT INTO OrderEventScheduler([InstanceId], StartedOn, [HEARTBEATON], [Vote], [Status], [State], iSDeleted)";
                 sqlQuery += $"    VALUES('{processorId}', Getdate(), GetDate(), 0, '','', 0)";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                await sqlcmd.ExecuteNonQueryAsync();
                sqlcmd.Dispose();
            }
        
        }



    }
}
