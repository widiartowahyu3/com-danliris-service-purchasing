﻿using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.Expedition;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.Expedition
{
    public class UnitPaymentOrderNotVerifiedReportFacade
    {
        private readonly PurchasingDbContext dbContext;
        private readonly DbSet<PurchasingDocumentExpedition> dbSet;
        public UnitPaymentOrderNotVerifiedReportFacade(PurchasingDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = this.dbContext.Set<PurchasingDocumentExpedition>();
        }

        public IQueryable<UnitPaymentOrderNotVerifiedReportViewModel> GetReportQuery(string no, string supplier, string division, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int offset)
        {
            DateTimeOffset dateFromFilter = (dateFrom == null ? new DateTime(1970, 1, 1) : dateFrom.Value.Date);
            DateTimeOffset dateToFilter = (dateTo == null ? DateTimeOffset.UtcNow.Date : dateTo.Value.Date);


            var Query = (from p in dbContext.PurchasingDocumentExpeditions
                         where p.IsDeleted == false &&
                            p.UnitPaymentOrderNo == (string.IsNullOrWhiteSpace(no) ? p.UnitPaymentOrderNo : no) &&
                            p.SupplierCode == (string.IsNullOrWhiteSpace(supplier) ? p.SupplierCode : supplier) &&
                            p.DivisionCode == (string.IsNullOrWhiteSpace(division) ? p.DivisionCode : division) &&
                            p.VerifyDate >= dateFromFilter &&
                            p.VerifyDate <= dateToFilter
                            && p.Position == (ExpeditionPosition)6
                         select new UnitPaymentOrderNotVerifiedReportViewModel
                         {
                             UnitPaymentOrderNo = p.UnitPaymentOrderNo,
                             DivisionName = p.DivisionName,
                             SupplierName = p.SupplierName,
                             VerifyDate = p.VerifyDate,
                             Currency = p.Currency,
                             UPODate = p.UPODate,
                             TotalPaid = p.TotalPaid,
                             NotVerifiedReason=p.NotVerifiedReason,
                             LastModifiedUtc=p.LastModifiedUtc
                         });
           
            return Query;
        }

        public Tuple<List<UnitPaymentOrderNotVerifiedReportViewModel>, int> GetReport(string no, string supplier, string division, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int page, int size, string Order, int offset)
        {
            var Query = GetReportQuery( no, supplier, division, dateFrom, dateTo, offset);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            if (OrderDictionary.Count.Equals(0))
            {
                Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            }


            Pageable<UnitPaymentOrderNotVerifiedReportViewModel> pageable = new Pageable<UnitPaymentOrderNotVerifiedReportViewModel>(Query, page - 1, size);
            List<UnitPaymentOrderNotVerifiedReportViewModel> Data = pageable.Data.ToList<UnitPaymentOrderNotVerifiedReportViewModel>();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData);
        }

        public MemoryStream GenerateExcel(string no, string supplier, string division, DateTimeOffset? dateFrom, DateTimeOffset? dateTo, int offset)
        {
            var Query = GetReportQuery(no, supplier, division, dateFrom, dateTo, offset);
            Query = Query.OrderByDescending(b => b.LastModifiedUtc);
            DataTable result = new DataTable();
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal Verifikasi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "No SPB", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Tanggal SPB", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Supplier", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Divisi", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Total Bayar", DataType = typeof(double) });
            result.Columns.Add(new DataColumn() { ColumnName = "Mata Uang", DataType = typeof(String) });
            result.Columns.Add(new DataColumn() { ColumnName = "Alasan", DataType = typeof(String) });
            
            if (Query.ToArray().Count() == 0)
                result.Rows.Add("", "", "", "", "", 0, "", ""); // to allow column name to be generated properly for empty data as template
            else
            {
                int index = 0;
                foreach (var item in Query)
                {
                    index++;
                    DateTimeOffset vDate= item.VerifyDate ?? new DateTime(1970, 1, 1);
                    string verifyDate = vDate == new DateTime(1970, 1, 1) ? "-" : vDate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID"));

                    result.Rows.Add(verifyDate,item.UnitPaymentOrderNo, item.UPODate.ToOffset(new TimeSpan(offset, 0, 0)).ToString("dd MMM yyyy", new CultureInfo("id-ID")), 
                        item.SupplierName, item.DivisionName, item.TotalPaid, item.Currency, item.NotVerifiedReason);
                }
            }

            return Excel.CreateExcel(new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") }, true);
        }

        
    }
}
