using BusinessLogic.DataModels;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Mysqlx.Crud;
using Mysqlx.Notice;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Ocsp;
using POManagementDataAccessLayer;
using POManagementDataAccessLayer.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace BusinessLogic
{
    public class POManagementBL
    {
        POManagementDAL dal;
        public POManagementBL(DbContextOptions<poMngtSQLContext> option)
        {
            dal = new POManagementDAL(option);
        }
        private string GetStatusOfPOItemAndPayments(sbyte value) {
            var status= PurchaseOrderItemAndPaymentStatus.Active.ToString();
            if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Draft)
            {
                status = PurchaseOrderItemAndPaymentStatus.Draft.ToString();
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Waiting)
            {
                status = PurchaseOrderItemAndPaymentStatus.Waiting.ToString();
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Completed)
            {
                status = PurchaseOrderItemAndPaymentStatus.Completed.ToString();
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Expired)
            {
                status = PurchaseOrderItemAndPaymentStatus.Expired.ToString();
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Declined)
            {
                status = PurchaseOrderItemAndPaymentStatus.Declined.ToString();
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Raised)
            {
                status = PurchaseOrderItemAndPaymentStatus.Raised.ToString();
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Claimed)
            {
                status = PurchaseOrderItemAndPaymentStatus.Claimed.ToString();
            }
            return status;
        }
        private PurchaseOrderItemAndPaymentStatus GetStatusOfPOItemAndPaymentsValue(sbyte value)
        {
            var status = PurchaseOrderItemAndPaymentStatus.Active;
            if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Draft)
            {
                status = PurchaseOrderItemAndPaymentStatus.Draft;
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Waiting)
            {
                status = PurchaseOrderItemAndPaymentStatus.Waiting;
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Completed)
            {
                status = PurchaseOrderItemAndPaymentStatus.Completed;
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Expired)
            {
                status = PurchaseOrderItemAndPaymentStatus.Expired;
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Declined)
            {
                status = PurchaseOrderItemAndPaymentStatus.Declined;
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Raised)
            {
                status = PurchaseOrderItemAndPaymentStatus.Raised;
            }
            else if (value == (sbyte)PurchaseOrderItemAndPaymentStatus.Claimed)
            {
                status = PurchaseOrderItemAndPaymentStatus.Claimed;
            }
            return status;
        }
        private List<string> GetDelayedElements(long poId, DateTime startedOn,out List<PayStatusDisplay> payStatus, out List<PayStatusDisplay> workStatus) 
        {
            var result= new List<string>();
            payStatus = new List<PayStatusDisplay>() { 
                new PayStatusDisplay(){ Pay=0, Status=PurchaseOrderItemAndPaymentStatus.Active
                },new PayStatusDisplay(){ Pay=0, Status=PurchaseOrderItemAndPaymentStatus.Completed
                },new PayStatusDisplay(){ Pay=0, Status=PurchaseOrderItemAndPaymentStatus.Waiting
                },new PayStatusDisplay(){ Pay=0, Status=PurchaseOrderItemAndPaymentStatus.Claimed,
                } };
            workStatus = new List<PayStatusDisplay>() {
                new PayStatusDisplay(){ Pay=0, Status=PurchaseOrderItemAndPaymentStatus.Active
                },new PayStatusDisplay(){ Pay=0, Status=PurchaseOrderItemAndPaymentStatus.Completed
                },new PayStatusDisplay(){ Pay=0, Status=PurchaseOrderItemAndPaymentStatus.Waiting
                }
            };
            var allPOItems= dal.GetAllPurchaseOrderLineItemsByPOIdAsync(poId).Result;
            foreach (var item in allPOItems) {
                var itemStatus = GetStatusOfPOItemAndPaymentsValue(item.LineItemStatus.Value);
                if ((itemStatus == PurchaseOrderItemAndPaymentStatus.Active ||
                    itemStatus == PurchaseOrderItemAndPaymentStatus.Waiting)
                    && startedOn.AddDays(item.LiItemCompletionDuration) < DateTime.Now)
                {
                    result.Add("Item:" + item.LiTitle + " is delayed by "
                        + DateTime.Now.Subtract(startedOn.AddDays(item.LiItemCompletionDuration)).Days + " days");
                }
                workStatus.Where(x => x.Status == GetStatusOfPOItemAndPaymentsValue(item.LineItemStatus.Value)).First().Pay += item.LiQuantity * item.LiRate.Value;
                
            }
            var allPOPayments = dal.GetAllPurchaseOrderPaymentsByPOIdAsync(poId).Result;
            foreach (var pay in allPOPayments)
            {
                var payStatusVal = GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus);
                if (payStatusVal != PurchaseOrderItemAndPaymentStatus.Completed
                    && pay.DueDate < DateTime.Now && pay.PaymentCompletionCriteria != 3)
                {
                    result.Add("Payment no:" + pay.SequenceNo + " is delayed by "
                        + DateTime.Now.Subtract(pay.DueDate).Days + " days");
                }
                else if (pay.PaymentCompletionCriteria == 3 && payStatusVal != PurchaseOrderItemAndPaymentStatus.Completed) 
                {
                    var itemList = dal.GetLineItemsFromPaymentIdAsync(pay.Id).Result;
                    if (itemList.All(x => GetStatusOfPOItemAndPaymentsValue(x.LineItemStatus.Value) == PurchaseOrderItemAndPaymentStatus.Completed)) 
                    {
                        var lastItemUpdate=itemList.Max(x => x.ModifiedOn);
                        result.Add("Payment no:" + pay.SequenceNo + " is delayed by "
                        + DateTime.Now.Subtract(lastItemUpdate).Days + " days");
                    }
                }
                payStatus.Where(x => x.Status == GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus)).First().Pay += pay.PaymentAmount.Value;
            }
            return result;
        }
        private async Task<List<PurchaseOrderShortDisplay>> GetPurchaseOrderDisplayData(List<PurchaseOrder> poLst) {
            
            var result= new List<PurchaseOrderShortDisplay>();
            foreach (var po in poLst) {
                var delayedPOElements = new List<string>();
                var payStatus = new List<PayStatusDisplay>() { new PayStatusDisplay() { Pay=0, Status=GetStatusOfPOItemAndPaymentsValue(po.PoStatus.Value) } };
                var workStatus = new List<PayStatusDisplay>() {new PayStatusDisplay() { Pay = 0, Status = GetStatusOfPOItemAndPaymentsValue(po.PoStatus.Value) } };
                if (GetStatusOfPOItemAndPaymentsValue(po.PoStatus.Value) == PurchaseOrderItemAndPaymentStatus.Active ||
                    GetStatusOfPOItemAndPaymentsValue(po.PoStatus.Value) == PurchaseOrderItemAndPaymentStatus.Waiting) 
                {
                    delayedPOElements = GetDelayedElements(po.Id, po.PoStartDate, out payStatus, out workStatus);
                }
                var raisedBy = await dal.GetUserByIdAsync(po.PoRaisedBy.Value);
                var raisedFor= await dal.GetUserByIdAsync(po.PoRaisedFor.Value);
                var status = GetStatusOfPOItemAndPayments(po.PoStatus.Value);
                result.Add(new PurchaseOrderShortDisplay()
                {
                    POAmount=po.PoTotalAmount,
                    PoId=po.Id,
                    RaisedById=raisedBy.Id,
                    RaisedByName=raisedBy.UserName,
                    RaisedByPhoneNo=raisedBy.UserSmsContact,
                    RaisedForId=raisedFor.Id,
                    RaisedForName=raisedFor.UserName,
                    RaisedForPhoneNo=raisedFor.UserSmsContact,
                    Status= status,
                    Title=po.PoTitle,
                    DelaysAndWaitingResponse=delayedPOElements,
                    PaymentStatus= payStatus,
                    WorkDoneStatus= workStatus

                });
            }
            return result;

        }
        public async Task<PurchaseOrderDetailedDisplay> GetPurchaseOrderDetailedDisplay(long poId) 
        {
            var result = new PurchaseOrderDetailedDisplay();
            var po= await dal.GetPurchaseOrderByIdAsync(poId);
            var poStatus=GetStatusOfPOItemAndPaymentsValue(po.PoStatus.Value);
            var raisedBy = await dal.GetUserByIdAsync(po.PoRaisedBy.Value);
            var raisedFor = await dal.GetUserByIdAsync(po.PoRaisedFor.Value);


            result.PoId = poId;
            result.Title=po.PoTitle;
            result.StartDate=po.PoStartDate;
            result.Status = GetStatusOfPOItemAndPayments(po.PoStatus.Value);
            result.Discount = po.PoDiscount;
            result.NotificationPeriodInDays = po.PoNotificationPeriodInDays;
            result.Description = po.PoDescription;
            result.POAmount=po.PoTotalAmount;
            var statusFlag = false;
            if (poStatus == PurchaseOrderItemAndPaymentStatus.Active
                || poStatus == PurchaseOrderItemAndPaymentStatus.Waiting
                || poStatus == PurchaseOrderItemAndPaymentStatus.Completed)
            {
                result.ExpectedCompletionDate = po.PoStartDate.AddDays(po.PoDurationInDays);
                statusFlag = true;
            }
            else
            {
                result.ExpectedCompletionDate = po.CreatedOn;
            }

            result.RaisedById = raisedBy.Id;
            result.RaisedByName = raisedBy.UserName;
            result.RaisedByPhoneNo = raisedBy.UserSmsContact;
            result.RaisedForId = raisedFor.Id;
            result.RaisedForName = raisedFor.UserName;
            result.RaisedForPhoneNo = raisedFor.UserSmsContact;

            result.POLineItems = new List<LineItemShortDisplay>();
            result.POTaxes = new List<TaxDisplay>();
            result.PORemarks = new List<RemarksDisplay>();
            result.POPayments = new List<PaymentShortDisplay>();
            result.POAttachments = new List<POAttachmentsDisplay>();
            result.POTermsAndConditions = new List<TermsAndConditionDisplay>();
            var poItems = await dal.GetAllPurchaseOrderLineItemsByPOIdAsync(poId);
            foreach (var i in poItems)
            {
                var tempItem = new LineItemShortDisplay()
                {
                    Description = i.LiDescription,
                    ItemCompletionDate = statusFlag ? po.PoStartDate.AddDays(i.LiItemCompletionDuration) : po.CreatedOn,
                    LineItemId = i.Id,
                    LineItemStatus = GetStatusOfPOItemAndPayments(i.LineItemStatus.Value),
                    Quantity = i.LiQuantity,
                    Rate = i.LiRate,
                    Title = i.LiTitle,
                    Remarks = new List<RemarksDisplay>()
                };
                var itemRemarks=await dal.GetAllLineItemRemarksByItemIdAsync(i.Id);
                foreach (var r in itemRemarks)
                {
                    var itemRemarkAttachments = await dal.GetAllLineItemRemarkAttachmentsByRemarkIdAsync(i.Id);
                    var tempRemark = new RemarksDisplay() {
                        Id = r.Id,
                        Description=r.RemarkTxt,
                        RemarkDate=r.CreatedOn,
                        Attachments=new List<GeneralAttachments>()
                    };
                    foreach (var a in itemRemarkAttachments)
                    {
                        tempRemark.Attachments.Add(new GeneralAttachments() {
                            Id=a.Id,
                            Link=a.AttachmentLink
                        });
                    }
                    tempItem.Remarks.Add(tempRemark);
                }
                result.POLineItems.Add(tempItem);
            }
            if (statusFlag)
            {
                var payList = await dal.GetAllPurchaseOrderPaymentsByPOIdAsync(poId);
                foreach (var p in payList)
                {
                    var tempPay = new PaymentShortDisplay()
                    {
                        PaymentAmount = p.PaymentAmount.Value,
                        DueDate = p.DueDate,
                        PaymentId = p.Id,
                        PaymentNotes = p.PaymentNotes,
                        PaymentStatus = GetStatusOfPOItemAndPayments(p.PaymentStatus),
                        SequenceNo = p.SequenceNo,
                        LineItemsRelation = new List<string>(),
                        Remarks = new List<RemarksDisplay>()
                    };
                    var payRemarks= await dal.GetAllPaymentRemarksByPayIdAsync(p.Id);
                    foreach (var payRemark in payRemarks)
                    {
                        var tempPayRemark = new RemarksDisplay() {
                            Id = payRemark.Id,
                            Description= payRemark.RemarkTxt,
                            RemarkDate=payRemark.CreatedOn,
                            Attachments=new List<GeneralAttachments>()
                        };
                        var remarkAttachments=await dal.GetAllPaymentRemarkAttachmentsByRemarkIdAsync(payRemark.Id);
                        foreach (var a in remarkAttachments)
                        {
                            tempPayRemark.Attachments.Add(new GeneralAttachments() { 
                                Id=a.Id,
                                Link=a.AttachmentLink
                            });
                        }
                    }
                    if (p.PaymentCompletionCriteria == 3) 
                    {
                        var allItems=await dal.GetLineItemsFromPaymentIdAsync(p.Id);
                        foreach (var i in allItems)
                        {
                            tempPay.LineItemsRelation.Add(i.LiTitle);
                        }
                    }
                    result.POPayments.Add(tempPay);
                }
            }
            else {
                var payList=await dal.GetAllPOPreapprvedPaymentsByPOIdAsync(poId);
                foreach (var p in payList)
                {
                    var tempPay = new PaymentShortDisplay()
                    {
                        PaymentAmount = p.PaymentAmount.Value,
                        DueDate = po.CreatedOn,
                        PaymentId = p.Id,
                        PaymentNotes = p.PaymentNotes,
                        PaymentStatus = GetStatusOfPOItemAndPayments(p.PaymentStatus),
                        SequenceNo = p.SequenceNo,
                        LineItemsRelation = new List<string>(),
                        Remarks = new List<RemarksDisplay>()
                    };
                    if (p.PaymentCompletionCriteria == 3)
                    {
                        var allItems = p.ExtraInformation.Split(',');
                        foreach (var i in allItems)
                        {
                            tempPay.LineItemsRelation.Add(i);
                        }
                    }
                    result.POPayments.Add(tempPay);
                }
            }
            var poTaxes=await dal.GetAllPurchaseOrderTaxesByPOIdAsync(poId);
            foreach (var t in poTaxes)
            {
                result.POTaxes.Add(new TaxDisplay()
                {
                    Id=t.Id,
                    Name=t.TaxTitle,
                    Percent=t.TaxPercentage
                });
            }
            var poTermsAndConditions = await dal.GetAllPurchaseTermsAndConditionsByPOIdAsync(poId);
            foreach (var tAndC in poTermsAndConditions)
            {
                result.POTermsAndConditions.Add(new TermsAndConditionDisplay()
                {
                    Id = tAndC.Id,
                    Description=tAndC.TermsAndConditionsValue,
                    Sequence=tAndC.SequenceNo
                });
            }
            var poRemarks = await dal.GetAllPurchaseOrderRemarksByPOIdAsync(poId);
            foreach (var r in poRemarks)
            {
                var tempRemark = new RemarksDisplay()
                {
                    Id = r.Id,
                    Description = r.RemarkTxt,
                    RemarkDate = r.CreatedOn,
                    Attachments = new List<GeneralAttachments>()
                };
                var remarkAttachments = await dal.GetAllPORemarkAttachmentsByRemarkIdAsync(r.Id);
                foreach (var rA in remarkAttachments)
                {
                    tempRemark.Attachments.Add(new GeneralAttachments() { 
                        Id = rA.Id,
                        Link=rA.AttachmentLink
                    });
                }
                result.PORemarks.Add(tempRemark);
            }
            var poAttachments = await dal.GetAllPurchaseOrderAttachmentsByPOIdAsync(poId);
            foreach (var oA in poAttachments)
            {
                result.POAttachments.Add(new POAttachmentsDisplay()
                {
                    Id=oA.Id,
                    Link=oA.AttachmentLink,
                    AttachmentType=oA.AttachmentType==1?"Terms Attachment":"Normal Attachment"
                });
            }
            return result;
        }
        public async Task<List<PurchaseOrderShortDisplay>> GetPurchaseOrderRaisedBy(long usrId) 
        {
            var poLst = await dal.GetAllPurchaseOrdersRaisedByAsync(usrId);
            return  await GetPurchaseOrderDisplayData(poLst);
        }
        public async Task<List<PurchaseOrderShortDisplay>> GetPurchaseOrderRaisedFor(long usrId) {
            var poLst = await dal.GetAllPurchaseOrdersRaisedForAsync(usrId);
            return await GetPurchaseOrderDisplayData(poLst);
        }
        public async Task AddUserActivity(string description, long poId, long uId, sbyte type)
        {
            await dal.CreateUserActivityLogAsync(new UserActivityLog()
            {
                ActionDescription = description,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                OpId = poId,
                UserId = uId,
                ActionType = type
            });

        }
        public async Task<List<UserSuggestion>> GetUserSuggestions(string stringParts)
        {
            var suggestedUsers=dal.GetUsersByPhoneStringPart(stringParts);
            var result= new List<UserSuggestion>();
            foreach (var u in suggestedUsers)
            {
                result.Add(new UserSuggestion() { Name = u.UserName, PhoneNumber = u.UserSmsContact });
            }
            return result;
        }
        public async Task<long> GetUserOrAddUser(string phoneNumber)
        {
            var user = await dal.GetUserByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                user = await dal.CreateUserAsync(new User()
                {
                    CreateOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    UserEmail = "",
                    UserGstin = "",
                    UserSmsContact = phoneNumber,
                    UserWatsappContact = phoneNumber,
                    UserName = "Temp Name",
                    UserStatus = true
                });
            }
            return user.Id;
        }
        private string GenerateRandomOTP() 
        {
            var random = new Random();
            var result = "";
            for (int i = 0; i < 6; i++)
            {
                result+= random.Next(0, 10).ToString();
            }
            return result;
        }
        public async Task<string> AddNewOTP(long uId) 
        {
            var generateOTP = await dal.CreateUsersOTPAsync(new UsersOtp()
            {
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                UserId = uId,
                UserOtp = GenerateRandomOTP(),
                UserOtpStatus = 0
            });
            return generateOTP.UserOtp;
            
        }
        public async Task<long> GetUserIdByPhoneNumber(string phoneNo) 
        {
            try
            {
                var user = await dal.GetUserByPhoneNumberAsync(phoneNo);
                if (user != null) 
                {
                    return user.Id;
                }
                return 0;
            }
            catch (Exception e)
            {
                return 0;
            }
            
        }
        public async Task<bool> ValidateOTP(long uid, string oTP, bool expiryIsValid=true)
        {
            if (expiryIsValid) 
            {
                await dal.ChangeStatusOfExpiredOTPs(uid);
            }
            return await dal.ValidateOTP(uid, oTP, expiryIsValid);
        }
        public async Task<bool> UpdateClient(Client c)
        {
            var user = await dal.GetUserByIdAsync(c.Id);
            if (user == null)
            {
                return false;
            }
            user.ModifiedOn = DateTime.Now;
            user.UserEmail = c.Email;
            user.UserGstin = c.GSTIN;
            user.UserSmsContact = c.PhoneNumber;
            user.UserWatsappContact = c.WatsappNumber;
            user.UserName = c.Name;
            user = await dal.UpdateUserAsync(user);
            if (user != null)
            {
                return true;
            }
            return false;
        }
        public async Task<bool> BlockAndUnblockClient(long uid, bool blockStatus)
        {
            var user = await dal.GetUserByIdAsync(uid);
            if (user != null)
            {
                user.UserStatus = !blockStatus;
                user.ModifiedOn = DateTime.Now;
                user = await dal.UpdateUserAsync(user);
                if (user != null)
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<Client> GetClient(long uid)
        {
            var usr = await dal.GetUserByIdAsync(uid);
            if (usr == null)
            {
                return new Client()
                {
                    Name = "Invalid User",
                    Status = "Invalid"
                };
            }
            if (usr.UserStatus == false)
            {
                return new Client()
                {
                    Name = "Blocked user",
                    Status = "Blocked"
                };
            }
            return new Client()
            {
                Id = usr.Id,
                GSTIN = usr.UserGstin,
                Email = usr.UserEmail,
                PhoneNumber = usr.UserSmsContact,
                WatsappNumber = usr.UserWatsappContact,
                Name = usr.UserName,
                Status = usr.CreateOn == usr.ModifiedOn ? "New" : "Active"
            };
        }
        private async Task<long> UpdateStatusPurchaseOrder(long id, PurchaseOrderItemAndPaymentStatus status)
        {
            var tempObj = await dal.GetPurchaseOrderByIdAsync(id);
            if (tempObj != null)
            {
                tempObj.PoStatus =(sbyte)status;
                tempObj.ModifiedOn = DateTime.Now;
                tempObj = await dal.AddOrUpdatePurchaseOrderAsync(tempObj);
                return tempObj.Id;
            }
            return 0;
        }
        public async Task<long> AddOrUpdatePurchaseOrder(PO order)
        {
            if (order != null)
            {
                var newPurchaseOrder = new PurchaseOrder
                {
                    PoTotalAmount = order.PoTotalAmount,
                    ModifiedOn = DateTime.Now,
                    PoDescription = order.PoDescription,
                    PoDiscount = order.PoDiscount,
                    PoStartDate = order.PoExpectedStartDate,
                    PoDurationInDays = order.PoCompletionDurationInDays,
                    PoNotificationPeriodInDays = order.PoNotificationPeriod,
                    PoRaisedBy = order.PoRaisedBy,
                    PoRaisedFor = order.PoRaisedFor,
                    PoTitle = order.PoTitle,
                    CreatedOn = DateTime.Now,
                    PoStatus = 0
                };
                var tempObj = await dal.GetPurchaseOrderByIdAsync(order.Id);
                if (tempObj != null)
                {
                    newPurchaseOrder.CreatedOn = tempObj.CreatedOn;
                    newPurchaseOrder.Id = tempObj.Id;
                    newPurchaseOrder.PoRaisedBy = tempObj.PoRaisedBy;
                }

                var poDal = await dal.AddOrUpdatePurchaseOrderAsync(newPurchaseOrder);
                if (poDal != null)
                {
                    return poDal.Id;
                }

            }
            return 0;

        }
        public async Task<long> AddOrUpdateItem(LineItem li)
        {
            if (li != null)
            {
                var newLineItem = new PurchaseOrderLineItem
                {
                    LiDescription = li.LiDescription,
                    LiItemCompletionDuration = li.LiItemCompletionInDays,
                    LineItemStatus = 0,
                    LiQuantity = li.LiQuantity,
                    LiRate = li.LiRate,
                    LiTitle = li.LiTitle,
                    PurchaseOrderId = li.PurchaseOrderId,
                    ModifiedOn = DateTime.Now,
                    CreatedOn = DateTime.Now

                };
                var tempObj = await dal.GetPurchaseOrderLineItemsByIdAsync(li.Id);
                if (tempObj != null)
                {
                    newLineItem.CreatedOn = tempObj.CreatedOn;
                    newLineItem.Id = tempObj.Id;
                    tempObj = await dal.UpdatePurchaseOrderLineItemsAsync(newLineItem);
                }
                else
                {
                    tempObj = await dal.CreatePurchaseOrderLineItemsAsync(newLineItem);
                }
                return tempObj.Id;

            }
            return 0;
        }
        //-----------------------------------------------------------------
        private async Task<long> UpdateStatusLineItem(long id, PurchaseOrderItemAndPaymentStatus status)
        {
            var tempObj = await dal.GetPurchaseOrderLineItemsByIdAsync(id);
            if (tempObj != null)
            {
                tempObj.LineItemStatus = (sbyte)status;
                tempObj.ModifiedOn = DateTime.Now;
                tempObj = await dal.UpdatePurchaseOrderLineItemsAsync(tempObj);
                return tempObj.Id;
            }
            return 0;
        }
        // Example usage: Create Item Status Change
        //Draft=>Raise -->Raise --Completed (Will Happen With PO)
        //Raise=>Draft --> Reconsider --Completed (Will Happen With PO)
        //Raise=>Active--> Accepted --Completed (Will Happen With PO)
        //Active=>Waiting --> DeliveryDoneClaimPay --
        //Waiting=>Active --> DeliveryUndoneUnclaimPay --
        //Waiting=>Completed--> CompletePayClaimed -- 
        public async Task<long> DeliveryDoneClaimPayLineItemStatusChange(long id) 
        {
            var itemId= await UpdateStatusLineItem(id, PurchaseOrderItemAndPaymentStatus.Waiting);
            var payments = await dal.GetAllPaymentItemRelationsByItemIdAsync(id);
            foreach (var pay in payments) {
                var allItems = await dal.GetLineItemsFromPaymentIdAsync(pay.PaymentId.Value);
                if (allItems.All(x => x.LineItemStatus == (sbyte)PurchaseOrderItemAndPaymentStatus.Waiting ||
                x.LineItemStatus == (sbyte)PurchaseOrderItemAndPaymentStatus.Completed)) 
                {
                    await AskForPayAndPayNotDonePaymentStatusChange(pay.PaymentId.Value);
                }
                
            }
            return itemId;
            
        }
        public async Task<long> DeliveryUndoneUnclaimPayLineItemStatusChange(long id)
        {
            var itemId= await UpdateStatusLineItem(id, PurchaseOrderItemAndPaymentStatus.Active);
            var payments = await dal.GetAllPaymentItemRelationsByItemIdAsync(id);
            foreach (var pay in payments) {
                await InvalidPayAskPaymentStatusChange(pay.PaymentId.Value);
            }
            return itemId;
        }
        public async Task<long> CompletePayClaimedLineItemStatusChange(long id)
        {
            return await UpdateStatusLineItem(id, PurchaseOrderItemAndPaymentStatus.Completed);
        }
        //------------------------------------------------------------------

        public async Task<long> AddPurchaseOrderAttachment(long orderId, char attachmentType, string attachmentLink)
        {
            //N- normal attachment, T- terms and condition
            var attachment = await dal.CreatePurchaseOrderAttachmentsAsync(new PurchaseOrderAttachment()
            {
                AttachmentLink = attachmentLink,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                PurchaseOrderId = orderId,
                AttachmentType = (sbyte)(attachmentType == 'N' ? 0 : (attachmentType == 'T' ? 1 : 0))
            });
            return attachment.Id;
        }
        public async Task<long> AddLineItemAttachment(long lineItemId, string attachmentLink)
        {
            var attachment = await dal.CreatePurchaseOrderLineItemsAttachmentsAsync(new PurchaseOrderLineItemsAttachment()
            {
                AttachmentLink = attachmentLink,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                LineItemId = lineItemId
            });
            return attachment.Id;
        }
        public async Task<long> AddRemarks(string remark, char parent, long parentId, List<string> attachmentLinks)
        {
            if (parent == 'I')
            {
                var obj = await dal.CreateLineItemRemarksAsync(new LineItemRemark()
                {
                    CreatedOn = DateTime.Now,
                    LineItemId = parentId,
                    ModifiedOn = DateTime.Now,
                    RemarkTxt = remark
                });
                foreach (var l in attachmentLinks)
                {
                    await dal.CreateLineItemRemarkAttachmentsAsync(new LineItemRemarkAttachment()
                    {
                        CreatedOn = DateTime.Now,
                        AttachmentLink = l,
                        ModifiedOn = DateTime.Now,
                        RemarkId = obj.Id
                    });
                }
                return obj.Id;
            }
            else if (parent == 'O')
            {
                var obj = await dal.CreatePurchaseOrderRemarkAsync(new PurchaseOrderRemark()
                {
                    CreatedOn = DateTime.Now,
                    PurchaseOrderId = parentId,
                    ModifiedOn = DateTime.Now,
                    RemarkTxt = remark
                });
                foreach (var l in attachmentLinks)
                {
                    await dal.CreatePurchaseOrderRemarkAttachmentAsync(new PurchaseOrderRemarkAttachment()
                    {
                        CreatedOn = DateTime.Now,
                        AttachmentLink = l,
                        ModifiedOn = DateTime.Now,
                        RemarkId = obj.Id
                    });
                }
                return obj.Id;
            }
            else if (parent == 'P')
            {
                var obj = await dal.CreatePaymentRemarkAsync(new PaymentRemark()
                {
                    CreatedOn = DateTime.Now,
                    PaymentId = parentId,
                    ModifiedOn = DateTime.Now,
                    RemarkTxt = remark
                });
                foreach (var l in attachmentLinks)
                {
                    await dal.CreatePaymentRemarkAttachmentAsync(new PaymentRemarkAttachment()
                    {
                        CreatedOn = DateTime.Now,
                        AttachmentLink = l,
                        ModifiedOn = DateTime.Now,
                        RemarkId = obj.Id
                    });
                }
                return obj.Id;
            }
            return 0;
        }
        public async Task<long> AddTermsAndConditions(string val, int seq, long poId)
        {
            var obj = await dal.CreatePurchaseTermsAndConditionAsync(new PurchaseTermsAndCondition()
            {
                CreatedOn = DateTime.Now,
                TermsAndConditionsValue = val,
                ModifiedOn = DateTime.Now,
                PurchaseOrderId = poId,
                SequenceNo = seq
            });
            return obj.Id;
        }
        public async Task<long> UpdateTermsAndConditions(long termId, string val, int seq)
        {
            var term = await dal.GetPurchaseTermsAndConditionByIdAsync(termId);
            term.SequenceNo = seq;
            term.ModifiedOn = DateTime.Now;
            term.TermsAndConditionsValue = val;
            term = await dal.UpdatePurchaseTermsAndConditionAsync(term);
            return term.Id;
        }
        public async Task<long> AddPurchaseOrderTaxes(long poId, decimal percent, string title)
        {
            var tax = await dal.CreatePurchaseOrderTaxesAsync(new PurchaseOrderTaxis()
            {
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                PurchaseOrderId = poId,
                TaxPercentage = percent,
                TaxTitle = title
            });
            return tax.Id;
        }
        public async Task<long> UpdatePurchaseOrderTaxes(long taxId, decimal percent, string title)
        {
            var tax = await dal.GetPurchaseOrderTaxesByIdAsync(taxId);
            tax.TaxPercentage = percent;
            tax.ModifiedOn = DateTime.Now;
            tax.TaxTitle = title;
            tax = await dal.UpdatePurchaseOrderTaxesAsync(tax);
            return tax.Id;
        }
        internal async Task<long> AddPayment(OrderPaymentPreApproval op)
        {
            var savedCriteria = 1;
            if (op.Criteria == 'F')
            {
                savedCriteria = 2;
            }
            else if (op.Criteria == 'I')
            {
                savedCriteria = 3;
            }
            var pay = await dal.CreatePaymentPreApprovedAsync(new PurchaseOrderPreApprovedPayment()
            {
                PaymentAmount = op.Amount,
                CreatedOn = DateTime.Now,
                ModifiedOn = DateTime.Now,
                PaymentCompletionCriteria = (sbyte)savedCriteria,
                PaymentNotes = op.Notes,
                PurchaseOrderId = op.PoId,
                SequenceNo = op.Sequence,
                PaymentStatus = 0,
                ExtraInformation = op.ExtraInfo
            });
            return pay.Id;
        }
        internal async Task<long> EditPayment(long payId,OrderPaymentPreApproval op)
        {
            var savedCriteria = 1;
            if (op.Criteria == 'F')
            {
                savedCriteria = 2;
            }
            else if (op.Criteria == 'I')
            {
                savedCriteria = 3;
            }
            var pay = await dal.GetPaymentPreApprovedByIdAsync(payId);
            if (pay != null) 
            {
                pay.PurchaseOrderId = op.PoId;
                pay.PaymentCompletionCriteria = (sbyte)savedCriteria;
                pay.PaymentNotes = op.Notes;
                pay.SequenceNo = op.Sequence;
                pay.ExtraInformation = op.ExtraInfo;
                pay.ModifiedOn = DateTime.Now;
                pay.PaymentAmount = op.Amount;
                pay=await dal.UpdatePaymentPreApprovedAsync(pay);
                return pay.Id;
            }
            return 0;
            
        }
        public async Task<long> AddPreApprovalPaymentFrequencyAndPartBased(long poId,int seq, string note, decimal amt, int frq, char paymentType)
        {
            var validPaymentTypes = new List<char>() { 'M', 'W', 'Q','A','F','P' };
            if (validPaymentTypes.Contains(paymentType)) 
            {
                var criteria = 'F';
                if (paymentType == 'A' || paymentType == 'F' || paymentType == 'P') 
                {
                    criteria = 'B';
                }
                return await AddPayment(new OrderPaymentPreApproval() {
                    Amount = amt,
                    Criteria= criteria,
                    ExtraInfo= frq.ToString() + ","+ paymentType.ToString(),
                    Notes=note,
                    PoId=poId,
                    Sequence=seq
                });
            }
            return 0;
        }
        public async Task<long> AddPreApprovalPaymentItemBased(long poId, int seq, string note, decimal amt, List<long> itemIds)
        {
            var info = "";
            itemIds.ForEach(x => info += x + ",");
            info = info.Substring(0, info.Length - 1);
            return await AddPayment(new OrderPaymentPreApproval()
            {
                Amount = amt,
                Criteria = 'I',
                ExtraInfo = info,
                Notes = note,
                PoId = poId,
                Sequence = seq
            });
        }
        public async Task<long> EditPreApprovalPaymentFrequencyAndPartBased(long payId,long poId, int seq, string note, decimal amt, int frq, char paymentType)
        {
            var validPaymentTypes = new List<char>() { 'M', 'W', 'Q', 'A', 'F', 'P' };
            if (validPaymentTypes.Contains(paymentType))
            {
                var criteria = 'F';
                if (paymentType == 'A' || paymentType == 'F' || paymentType == 'P')
                {
                    criteria = 'B';
                }
                return await EditPayment(payId, new OrderPaymentPreApproval()
                {
                    Amount = amt,
                    Criteria = criteria,
                    ExtraInfo = frq.ToString() + "," + paymentType.ToString(),
                    Notes = note,
                    PoId = poId,
                    Sequence = seq
                });
            }
            return 0;
        }
        public async Task<long> EditPreApprovalPaymentItemBased(long payId, long poId, int seq, string note, decimal amt, List<long> itemIds)
        {
            var info = "";
            itemIds.ForEach(x => info += x + ",");
            info = info.Substring(0, info.Length - 1);
            return await EditPayment(payId,new OrderPaymentPreApproval()
            {
                Amount = amt,
                Criteria = 'I',
                ExtraInfo = info,
                Notes = note,
                PoId = poId,
                Sequence = seq
            });
        }

        //--------------------------------------------------------------------------------------------
        private async Task<long> ChangeStatusOfPreApprovedPayments(long payId, PurchaseOrderItemAndPaymentStatus orderStatus) 
        {
            var pay=await dal.GetPaymentPreApprovedByIdAsync(payId);
            pay.ModifiedOn = DateTime.Now;
            pay.PaymentStatus = (sbyte)orderStatus;
            pay= await dal.UpdatePaymentPreApprovedAsync(pay);
            return pay.Id;
        }
        private async Task<long> ChangeStatusOfPayments(long payId, PurchaseOrderItemAndPaymentStatus orderStatus)
        {
            var pay = await dal.GetPurchaseOrderPaymentByIdAsync(payId);
            pay.ModifiedOn = DateTime.Now;
            pay.PaymentStatus = (sbyte)orderStatus;
            pay = await dal.UpdatePurchaseOrderPaymentAsync(pay);
            return pay.Id;
        }
        // Example usage: Create Item Status Change
        //Draft=>Raise -->Raise --Completed (Will Happen With PO)
        //Raise=>Draft --> Reconsider --Completed (Will Happen With PO)
        //Raise=>Active--> Accepted --Completed (Will Happen With PO)
        //Active=>Waiting --> PayAsked --
        //Waiting=>Active --> InvalidPayAsk --
        //Waiting=>Claimed --> ClaimPay --
        //Claimed=>Waiting --> PayNotDone -- 
        //Claimed=>Completed--> PayClaimAccepted --
        public async Task<long> AskForPayAndPayNotDonePaymentStatusChange(long payId) 
        {
            return await ChangeStatusOfPayments(payId, PurchaseOrderItemAndPaymentStatus.Waiting);
        }
        public async Task<long> InvalidPayAskPaymentStatusChange(long payId)
        {
            return await ChangeStatusOfPayments(payId, PurchaseOrderItemAndPaymentStatus.Active);
        }
        public async Task<long> ClaimPayPaymentStatusChange(long payId)
        {
            return await ChangeStatusOfPayments(payId, PurchaseOrderItemAndPaymentStatus.Claimed);
        }
        public async Task<long> PayClaimAcceptedPaymentStatusChange(long payId)
        {
            return await ChangeStatusOfPayments(payId, PurchaseOrderItemAndPaymentStatus.Completed);
        }
        //--------------------------------------------------------------------------------------------
        //----------------------------------------------------------------------------------------------

        private async Task<long> UpdatePurchaseOrderStatusAsync(long poId, PurchaseOrderItemAndPaymentStatus orderStatus)
        {
            var id = await UpdateStatusPurchaseOrder(poId, orderStatus);
            var lineItemList = await dal.GetAllPurchaseOrderLineItemsByPOIdAsync(id);

            if (orderStatus == PurchaseOrderItemAndPaymentStatus.Waiting)
            {
                foreach (var lineItem in lineItemList)
                {
                    if (GetStatusOfPOItemAndPaymentsValue(lineItem.LineItemStatus.Value) == PurchaseOrderItemAndPaymentStatus.Active)
                    {
                        await UpdateStatusLineItem(lineItem.Id, orderStatus);
                    }
                }
                var payments = await dal.GetAllPOPreapprvedPaymentsByPOIdAsync(id);
                foreach (var pay in payments)
                {
                    if (GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus) == PurchaseOrderItemAndPaymentStatus.Active ||
                        GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus) == PurchaseOrderItemAndPaymentStatus.Claimed)
                    {
                        await ChangeStatusOfPreApprovedPayments(pay.Id, orderStatus);
                    }

                }
                var activePayments = await dal.GetAllPurchaseOrderPaymentsByPOIdAsync(poId);
                foreach (var pay in activePayments)
                {
                    if (GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus) == PurchaseOrderItemAndPaymentStatus.Active ||
                        GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus) == PurchaseOrderItemAndPaymentStatus.Claimed)
                    {
                        await ChangeStatusOfPayments(pay.Id, orderStatus);
                    }

                }

            }
            else if (orderStatus == PurchaseOrderItemAndPaymentStatus.Active)
            {
                foreach (var lineItem in lineItemList)
                {
                    if (GetStatusOfPOItemAndPaymentsValue(lineItem.LineItemStatus.Value) == PurchaseOrderItemAndPaymentStatus.Waiting)
                    {
                        await UpdateStatusLineItem(lineItem.Id, orderStatus);
                    }
                }
                var payments = await dal.GetAllPOPreapprvedPaymentsByPOIdAsync(id);
                foreach (var pay in payments)
                {
                    if (GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus) == PurchaseOrderItemAndPaymentStatus.Waiting)
                    {
                        await ChangeStatusOfPreApprovedPayments(pay.Id, orderStatus);
                    }

                }
                var activePayments = await dal.GetAllPurchaseOrderPaymentsByPOIdAsync(poId);
                foreach (var pay in activePayments)
                {
                    if (GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus) == PurchaseOrderItemAndPaymentStatus.Waiting)
                    {
                        await ChangeStatusOfPayments(pay.Id, orderStatus);
                    }
                }
            }
            else if (orderStatus == PurchaseOrderItemAndPaymentStatus.Claimed)
            {
                foreach (var lineItem in lineItemList)
                {
                    await UpdateStatusLineItem(lineItem.Id, PurchaseOrderItemAndPaymentStatus.Completed);
                }
                var payments = await dal.GetAllPOPreapprvedPaymentsByPOIdAsync(id);
                foreach (var pay in payments)
                {
                    if (GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus) == PurchaseOrderItemAndPaymentStatus.Waiting)
                    {
                        await ChangeStatusOfPreApprovedPayments(pay.Id, orderStatus);
                    }

                }
                var activePayments = await dal.GetAllPurchaseOrderPaymentsByPOIdAsync(poId);
                foreach (var pay in activePayments)
                {
                    if (GetStatusOfPOItemAndPaymentsValue(pay.PaymentStatus) == PurchaseOrderItemAndPaymentStatus.Waiting)
                    {
                        await ChangeStatusOfPayments(pay.Id, orderStatus);
                    }
                }
            }
            else
            {
                foreach (var lineItem in lineItemList)
                {
                    await UpdateStatusLineItem(lineItem.Id, orderStatus);
                }
                var payments = await dal.GetAllPOPreapprvedPaymentsByPOIdAsync(id);
                foreach (var pay in payments)
                {
                    await ChangeStatusOfPreApprovedPayments(pay.Id, orderStatus);
                }
            }
            if (orderStatus == PurchaseOrderItemAndPaymentStatus.Completed)
            {
                var activePayments = await dal.GetAllPurchaseOrderPaymentsByPOIdAsync(poId);
                foreach (var pay in activePayments)
                {
                    await ChangeStatusOfPayments(pay.Id, orderStatus);
                }
            }

            return id;
        }

        // Example usage: Create PO Status Change
        //Draft=>Raise -->Raise --Completed
        //Raise=>Draft --> Reconsider --Completed
        //Raise=>Active--> Accepted --Completed
        //Active=>Waiting --> DeliveryDoneOrPayUndone --Completed
        //Waiting=>Active --> DeliveryUndone --Completed
        //Waiting=>Claimed --> DeliveryReceivedPayDone --Completed
        //Claimed=>Waiting --> DeliveryDoneOrPayUndone --Completed
        //Claimed=>Completed--> Complete --Completed

        public async Task<long> RaisePurchaseOrderAsync(long poId)
        {
            return await UpdatePurchaseOrderStatusAsync(poId, PurchaseOrderItemAndPaymentStatus.Raised);
        }

        public async Task<long> ReconsiderPurchaseOrderAsync(long poId)
        {
            return await UpdatePurchaseOrderStatusAsync(poId, PurchaseOrderItemAndPaymentStatus.Draft);
        }

        public async Task<long> DeclinePurchaseOrderAsync(long poId)
        {
            return await UpdatePurchaseOrderStatusAsync(poId, PurchaseOrderItemAndPaymentStatus.Declined);
        }
        public async Task<long> DeliveryDoneOrPayUndonePurchaseOrderAsync(long poId)
        {
            return await UpdatePurchaseOrderStatusAsync(poId, PurchaseOrderItemAndPaymentStatus.Waiting);
        }
        public async Task<long> DeliveryUndonePurchaseOrderAsync(long poId)
        {
            return await UpdatePurchaseOrderStatusAsync(poId, PurchaseOrderItemAndPaymentStatus.Active);
        }
        public async Task<long> DeliveryReceivedPurchaseOrderAsync(long poId)
        {
            return await UpdatePurchaseOrderStatusAsync(poId, PurchaseOrderItemAndPaymentStatus.Claimed);
        }
        public async Task<long> CompletePurchaseOrder(long poId) 
        {
            return await UpdatePurchaseOrderStatusAsync(poId, PurchaseOrderItemAndPaymentStatus.Completed);
        }
        
        //-----------------------------------------------------------------------------------------------

        public async Task<long> AcceptedPurchaseOrder(long poId)
        {
            decimal totalPaymentDone = 0.0m;
            var po = await dal.GetPurchaseOrderByIdAsync(poId);
            po.PoStartDate = DateTime.Now;
            po.ModifiedOn = DateTime.Now;
            po.PoStatus = (sbyte)PurchaseOrderItemAndPaymentStatus.Active;
            po=await dal.AddOrUpdatePurchaseOrderAsync(po);

            var itemList = await dal.GetAllPurchaseOrderLineItemsByPOIdAsync(po.Id);
            foreach (var item in itemList)
            {
                item.LineItemStatus = (sbyte)PurchaseOrderItemAndPaymentStatus.Active;
                item.ModifiedOn = DateTime.Now;
                await dal.UpdatePurchaseOrderLineItemsAsync(item);
            }
            var allPayments = await dal.GetAllPOPreapprvedPaymentsByPOIdAsync(po.Id);
            foreach (var p in allPayments)
            {
                p.PaymentStatus = (sbyte)PurchaseOrderItemAndPaymentStatus.Active;
                p.ModifiedOn = DateTime.Now;
                await dal.UpdatePaymentPreApprovedAsync(p);
                if (p.PaymentCompletionCriteria == 3)
                {
                    //Item Based Payment 
                    if (!string.IsNullOrEmpty(p.ExtraInformation)) 
                    {
                        var tempPayment = new PurchaseOrderPayment()
                        {
                            PaymentAmount = p.PaymentAmount,
                            CreatedOn = DateTime.Now,
                            ModifiedOn = DateTime.Now,
                            PaymentCompletionCriteria = p.PaymentCompletionCriteria,
                            PaymentNotes = p.PaymentNotes,
                            PaymentStatus = 1,
                            PurchaseOrderId = p.PurchaseOrderId,
                            SequenceNo = p.SequenceNo,
                        };
                        var allItems=p.ExtraInformation.Split(',');
                        var maxItemCompletionDuration = 0;
                        foreach (var i in allItems) {
                            var itemObj = await dal.GetPurchaseOrderLineItemsByIdAsync(long.Parse(i));
                            if (maxItemCompletionDuration <= itemObj.LiItemCompletionDuration) {
                                maxItemCompletionDuration=itemObj.LiItemCompletionDuration;
                            }
                        }
                        tempPayment.DueDate=DateTime.Now.AddDays(maxItemCompletionDuration);
                        var newPayment = await dal.CreatePurchaseOrderPaymentAsync(tempPayment);
                        totalPaymentDone += newPayment.PaymentAmount.Value;
                        foreach (var i in allItems) {

                            await dal.CreatePaymentItemRelationAsync(new PaymentItemRelation() {
                                CreatedOn= DateTime.Now,
                                LineItemId=long.TryParse(i,out long id)?id:null,
                                ModifiedOn= DateTime.Now,
                                PaymentId=newPayment.Id
                            });
                        }
                    }
                }
                else if (p.PaymentCompletionCriteria == 2)
                {
                    //Frequency based payment
                    var frequenceyInformation = p.ExtraInformation.Split(',');
                    var dueDate = DateTime.Now;
                    for (var i = 0; i < int.Parse(frequenceyInformation[0]); i++) 
                    {
                        var tempPayment = new PurchaseOrderPayment()
                        {
                            PaymentAmount = p.PaymentAmount,
                            CreatedOn = DateTime.Now,
                            ModifiedOn = DateTime.Now,
                            PaymentCompletionCriteria = p.PaymentCompletionCriteria,
                            PaymentNotes = p.PaymentNotes,
                            PaymentStatus = 1,
                            PurchaseOrderId = p.PurchaseOrderId,
                            SequenceNo = p.SequenceNo,
                        };
                        if (frequenceyInformation[1] == "W") 
                        {
                            dueDate = dueDate.AddDays(7);
                        }
                        else if(frequenceyInformation[1] == "M") {
                            dueDate = dueDate.AddMonths(1);
                        }
                        else if (frequenceyInformation[1] == "Q")
                        {
                            dueDate = dueDate.AddMonths(3);
                        }
                        tempPayment.DueDate = dueDate;
                        tempPayment=await dal.CreatePurchaseOrderPaymentAsync(tempPayment);
                        totalPaymentDone += tempPayment.PaymentAmount.Value;
                    }

                } else{
                    var tempPayment = new PurchaseOrderPayment()
                    {
                        PaymentAmount = p.PaymentAmount,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now,
                        PaymentCompletionCriteria = p.PaymentCompletionCriteria,
                        PaymentNotes = p.PaymentNotes,
                        PaymentStatus = 1,
                        PurchaseOrderId = p.PurchaseOrderId,
                        SequenceNo = p.SequenceNo,
                    };
                    // Normal Payment object
                    var basicPaymentParts=p.ExtraInformation.Split(',');
                    if (basicPaymentParts[1] == "A")
                    {
                        //Advance payment
                        totalPaymentDone += tempPayment.PaymentAmount.Value;
                        tempPayment.DueDate= DateTime.Now;
                    }
                    else if (basicPaymentParts[1] == "F")
                    {
                        if (totalPaymentDone + tempPayment.PaymentAmount != po.PoTotalAmount) 
                        {
                            tempPayment.PaymentAmount = po.PoTotalAmount - totalPaymentDone;
                        }
                        //Final Payment
                        tempPayment.DueDate = DateTime.Now.AddDays(po.PoDurationInDays);
                    }
                    else 
                    {
                        totalPaymentDone += tempPayment.PaymentAmount.Value;
                        tempPayment.DueDate = DateTime.Now.AddDays(int.Parse(basicPaymentParts[0]));
                        //Part Payment
                    }
                    await dal.CreatePurchaseOrderPaymentAsync(tempPayment);
                }
                
            }
            return po.Id;
        }
    }
}
