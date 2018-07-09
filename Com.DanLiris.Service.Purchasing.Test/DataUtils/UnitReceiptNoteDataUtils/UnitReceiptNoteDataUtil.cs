﻿using Com.DanLiris.Service.Purchasing.Lib.Facades.UnitReceiptNoteFacade;
using Com.DanLiris.Service.Purchasing.Lib.Models.DeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.DeliveryOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.IntegrationViewModel;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitReceiptNoteDataUtils
{
    public class UnitReceiptNoteDataUtil
    {
        private UnitReceiptNoteItemDataUtil unitReceiptNoteItemDataUtil;
        private DeliveryOrderDataUtil deliveryOrderDataUtil;
        private readonly UnitReceiptNoteFacade facade;

        public UnitReceiptNoteDataUtil(UnitReceiptNoteItemDataUtil unitReceiptNoteItemDataUtil, UnitReceiptNoteFacade facade, DeliveryOrderDataUtil deliveryOrderDataUtil)
        {
            this.unitReceiptNoteItemDataUtil = unitReceiptNoteItemDataUtil;
            this.deliveryOrderDataUtil = deliveryOrderDataUtil;
            this.facade = facade;
        }

        public async Task<Lib.Models.UnitReceiptNoteModel.UnitReceiptNote> GetNewData(string user)
        {
            DeliveryOrder deliveryOrder = await deliveryOrderDataUtil.GetTestData(user);
            List<DeliveryOrderItem> doItem = new List<DeliveryOrderItem> (deliveryOrder.Items);
            List<DeliveryOrderDetail> doDetail = new List<DeliveryOrderDetail>(doItem[0].Details);
            

            doItem[0].Details = doDetail;
            return new Lib.Models.UnitReceiptNoteModel.UnitReceiptNote
            {
                UnitId = doDetail[0].UnitId,
                UnitCode = doDetail[0].UnitCode,
                UnitName = "UnitName",
                Remark = "Test URN",
                SupplierCode = deliveryOrder.SupplierCode,
                SupplierId = deliveryOrder.SupplierId,
                SupplierName = deliveryOrder.SupplierName,
                ReceiptDate = DateTimeOffset.UtcNow,
                DOId=deliveryOrder.Id,
                DONo=deliveryOrder.DONo,
                Items = new List<Lib.Models.UnitReceiptNoteModel.UnitReceiptNoteItem> { unitReceiptNoteItemDataUtil.GetNewData(doItem) }
            };
        }

        public async Task<UnitReceiptNoteViewModel> GetNewDataViewModel(string user)
        {
            DeliveryOrder deliveryOrder = await deliveryOrderDataUtil.GetTestData(user);
            List<DeliveryOrderItem> doItem = new List<DeliveryOrderItem>(deliveryOrder.Items);
            return new UnitReceiptNoteViewModel
            {
                date=DateTimeOffset.Now,
                supplier = new SupplierViewModel
                {
                    _id = deliveryOrder.SupplierId,
                    code = deliveryOrder.SupplierCode,
                    name = deliveryOrder.SupplierName,
                    import=true
                },
                unit = new UnitViewModel
                {
                    _id = "UnitId",
                    code = "UnitCode",
                    name = "UnitName",
                    division = new DivisionViewModel
                    {
                        _id = "DivisionId",
                        code = "DivisionCode",
                        name = "DivisionName",
                    }
                },
                doNo=deliveryOrder.DONo,
                remark = "test",
                items = new List<UnitReceiptNoteItemViewModel> { unitReceiptNoteItemDataUtil.GetNewDataViewModel(doItem) }
            };
        }
        public async Task<Lib.Models.UnitReceiptNoteModel.UnitReceiptNote> GetTestData(string user)
        {
            Lib.Models.UnitReceiptNoteModel.UnitReceiptNote unitReceiptNote = await GetNewData(user);

            await facade.Create(unitReceiptNote, user);

            return unitReceiptNote;
        }
    }
}