using ASC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ASC.Model.Queries
{
    public static class Queries
    {
        public static Expression<Func<ServiceRequest, bool>> GetDashboardQuery(DateTime? requestedDate,
            List<string> status = null,
            string email = "",
            string serviceEngineerEmail = "")
        {
            // Khởi tạo query mặc định
            Expression<Func<ServiceRequest, bool>> query = u => true;

            if (requestedDate.HasValue)
            {
                query = query.And(u => u.RequestedDate >= requestedDate);
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.And(u => u.PartitionKey == email);
            }

            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                query = query.And(u => u.ServiceEngineer == serviceEngineerEmail);
            }

            if (status != null && status.Any())
            {
                Expression<Func<ServiceRequest, bool>> statusQueries = u => false;
                foreach (var state in status)
                {
                    var temp = state;
                    statusQueries = statusQueries.Or(u => u.Status == temp);
                }
                query = query.And(statusQueries);
            }

            return query;
        }
    }
}