using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace RaftImplementation.Repository
{
    public class OrderEventOutboxRepository : IOrderEventOutboxRepository
    {
        string constr = "Data Source=IN01LAP80323\\DEV2016;Initial Catalog=test;Integrated Security=True";
        public async Task<IList<int>> GetOrderEvents()
        {
            var orderEvents = new List<int>();
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"Select Top 10 Id from OrderEventOutbox Where STATUS='Submitted' AND IsDeleted = 0";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var sqlReader = await sqlcmd.ExecuteReaderAsync();
                while (sqlReader.Read())
                {
                    orderEvents.Add(Convert.ToInt32(sqlReader[0]));
                }
                sqlcmd.Dispose();
            }
            return orderEvents;
        }

        public async Task<int> GetOrderEvent(Guid instanceId)
        {
            int Id=0;
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();

                var sqlQuery = $"Select Id from OrderEventOutbox Where ProcessorId='{instanceId}' and Status not in ('InProgress','Published') AND IsDeleted=0 ";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                var result = await sqlcmd.ExecuteScalarAsync();
                if (result != null)
                {
                    Id = Convert.ToInt32(result); //.ToString();
                }                
                //sqlcmd.Dispose();

                sqlQuery = $"Update OrderEventOutbox set status = 'InProgress' Where Id={Id}";
                sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                await sqlcmd.ExecuteNonQueryAsync();
                sqlcmd.Dispose();
            }
            return Id;
        }


        public async Task SetProcessor(int orderEventId, Guid processorId)
        {
            
            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"Update OrderEventOutbox set ProcessorId = '{processorId}' Where Id = {orderEventId} and IsDeleted = 0 and Status='Submitted'";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                await sqlcmd.ExecuteNonQueryAsync();
               
                sqlcmd.Dispose();
            }
        }

        public async Task MarkOrderEventPublished(int orderEventId)
        {

            using (var sqlcon = new SqlConnection(constr))
            {
                sqlcon.Open();
                var sqlQuery = $"Update OrderEventOutbox set Status = 'Published',  IsDeleted = 1 Where Id = {orderEventId} and IsDeleted = 0";
                var sqlcmd = sqlcon.CreateCommand();
                sqlcmd.CommandText = sqlQuery;
                await sqlcmd.ExecuteNonQueryAsync();

                sqlcmd.Dispose();
            }
        }

     
    }
}
