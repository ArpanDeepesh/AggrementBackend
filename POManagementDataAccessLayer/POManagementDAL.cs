using Microsoft.EntityFrameworkCore;
using POManagementDataAccessLayer.DataAccessLayer;

namespace POManagementDataAccessLayer
{
    public class POManagementDAL
    {
        poMngtSQLContext _context;
        public POManagementDAL(DbContextOptions<poMngtSQLContext> option)
        {
            _context = new poMngtSQLContext(option);
        }
        public List<User> GetUsersByPhoneStringPart(string stringPart) {
            return _context.Users.Where(x => x.UserSmsContact.Contains(stringPart)).Take(10).ToList();
        }


        // CRUD Operations for User
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(long id)
        {
            return await _context.Users.FindAsync(id);
        }
        public async Task<User> GetUserByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.Where(x => x.UserSmsContact == phoneNumber).FirstOrDefaultAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for POAndItemStatus
        public async Task<List<PoAndItemStatus>> GetAllPOAndItemStatusesAsync()
        {
            return await _context.PoAndItemStatuses.ToListAsync();
        }

        public async Task<PoAndItemStatus> GetPOAndItemStatusByIdAsync(byte id)
        {
            return await _context.PoAndItemStatuses.FindAsync(id);
        }

        public async Task<PoAndItemStatus> CreatePOAndItemStatusAsync(PoAndItemStatus status)
        {
            _context.PoAndItemStatuses.Add(status);
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task<PoAndItemStatus> UpdatePOAndItemStatusAsync(PoAndItemStatus status)
        {
            _context.PoAndItemStatuses.Update(status);
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task<bool> DeletePOAndItemStatusAsync(byte id)
        {
            var status = await _context.PoAndItemStatuses.FindAsync(id);
            if (status == null)
            {
                return false;
            }

            _context.PoAndItemStatuses.Remove(status);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for UsersOTP
        public async Task<List<UsersOtp>> GetAllUsersOTPsAsync()
        {
            return await _context.UsersOtps.ToListAsync();
        }

        public async Task<UsersOtp> GetUsersOTPByIdAsync(long id)
        {
            return await _context.UsersOtps.FindAsync(id);
        }

        public async Task<UsersOtp> CreateUsersOTPAsync(UsersOtp usersOTP)
        {
            _context.UsersOtps.Add(usersOTP);
            await _context.SaveChangesAsync();
            return usersOTP;
        }

        public async Task<UsersOtp> UpdateUsersOTPAsync(UsersOtp usersOTP)
        {
            _context.UsersOtps.Update(usersOTP);
            await _context.SaveChangesAsync();
            return usersOTP;
        }

        public async Task<bool> DeleteUsersOTPAsync(long id)
        {
            var usersOTP = await _context.UsersOtps.FindAsync(id);
            if (usersOTP == null)
            {
                return false;
            }

            _context.UsersOtps.Remove(usersOTP);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ValidateOTP(long uid, string OTP,bool expiryIsValid=true)
        {
            var validOTP = _context.UsersOtps.Where(x => x.UserId == uid
            && x.UserOtpStatus == 0 && x.UserOtp == OTP
            && x.CreatedOn == x.ModifiedOn && expiryIsValid?x.CreatedOn.AddMinutes(20) > DateTime.Now:true).FirstOrDefault();
            if (validOTP != null)
            {
                validOTP.UserOtpStatus = 1;
                validOTP.ModifiedOn = DateTime.Now;
                await UpdateUsersOTPAsync(validOTP);
                return true;
            }
            return false;
        }
        public async Task<bool> ChangeStatusOfExpiredOTPs(long uid)
        {
            var allOTPs = await _context.UsersOtps.Where(x => x.UserId == uid && x.UserOtpStatus == 0).ToListAsync();
            foreach (var otp in allOTPs)
            {
                if (otp.CreatedOn.AddMinutes(20) < DateTime.Now)
                {
                    otp.UserOtpStatus = 2;
                    otp.ModifiedOn = DateTime.Now;
                    await UpdateUsersOTPAsync(otp);
                }
            }
            return true;
        }
        // CRUD Operations for PurchaseOrder
        public async Task<List<PurchaseOrder>> GetAllPurchaseOrdersAsync()
        {
            return await _context.PurchaseOrders.ToListAsync();
        }
        public async Task<List<PurchaseOrder>> GetAllPurchaseOrdersRaisedByAsync(long usrId)
        {
            return await _context.PurchaseOrders.Where(x=>x.PoRaisedBy==usrId).ToListAsync();
        }
        public async Task<List<PurchaseOrder>> GetAllPurchaseOrdersRaisedForAsync(long usrId)
        {
            return await _context.PurchaseOrders.Where(x => x.PoRaisedFor == usrId).ToListAsync();
        }

        public async Task<PurchaseOrder> GetPurchaseOrderByIdAsync(long id)
        {
            return await _context.PurchaseOrders.FindAsync(id);
        }

        public async Task<PurchaseOrder> AddOrUpdatePurchaseOrderAsync(PurchaseOrder purchaseOrder)
        {
            var existingOrder = await _context.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id);

            if (existingOrder == null)
            {
                // PurchaseOrder does not exist, so create it
                _context.PurchaseOrders.Add(purchaseOrder);
            }
            else
            {
                // PurchaseOrder exists, so update it
                _context.PurchaseOrders.Update(purchaseOrder);
            }

            await _context.SaveChangesAsync();
            return purchaseOrder;
        }


        public async Task<bool> DeletePurchaseOrderAsync(long id)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder == null)
            {
                return false;
            }

            _context.PurchaseOrders.Remove(purchaseOrder);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PurchaseOrderRemark
        public async Task<List<PurchaseOrderRemark>> GetAllPurchaseOrderRemarksAsync()
        {
            return await _context.PurchaseOrderRemarks.ToListAsync();
        }
        public async Task<List<PurchaseOrderRemark>> GetAllPurchaseOrderRemarksByPOIdAsync(long poId)
        {
            return await _context.PurchaseOrderRemarks.Where(x=>x.PurchaseOrderId==poId).ToListAsync();
        }
        public async Task<PurchaseOrderRemark> GetPurchaseOrderRemarkByIdAsync(long id)
        {
            return await _context.PurchaseOrderRemarks.FindAsync(id);
        }

        public async Task<PurchaseOrderRemark> CreatePurchaseOrderRemarkAsync(PurchaseOrderRemark purchaseOrderRemark)
        {
            _context.PurchaseOrderRemarks.Add(purchaseOrderRemark);
            await _context.SaveChangesAsync();
            return purchaseOrderRemark;
        }

        public async Task<PurchaseOrderRemark> UpdatePurchaseOrderRemarkAsync(PurchaseOrderRemark purchaseOrderRemark)
        {
            _context.PurchaseOrderRemarks.Update(purchaseOrderRemark);
            await _context.SaveChangesAsync();
            return purchaseOrderRemark;
        }

        public async Task<bool> DeletePurchaseOrderRemarkAsync(long id)
        {
            var purchaseOrderRemark = await _context.PurchaseOrderRemarks.FindAsync(id);
            if (purchaseOrderRemark == null)
            {
                return false;
            }

            _context.PurchaseOrderRemarks.Remove(purchaseOrderRemark);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PurchaseOrderRemarkAttachment
        public async Task<List<PurchaseOrderRemarkAttachment>> GetAllPurchaseOrderRemarkAttachmentsAsync()
        {
            return await _context.PurchaseOrderRemarkAttachments.ToListAsync();
        }

        public async Task<List<PurchaseOrderRemarkAttachment>> GetAllPORemarkAttachmentsByRemarkIdAsync(long remarkId)
        {
            return await _context.PurchaseOrderRemarkAttachments.Where(x=>x.RemarkId==remarkId).ToListAsync();
        }

        public async Task<PurchaseOrderRemarkAttachment> GetPurchaseOrderRemarkAttachmentByIdAsync(long id)
        {
            return await _context.PurchaseOrderRemarkAttachments.FindAsync(id);
        }

        public async Task<PurchaseOrderRemarkAttachment> CreatePurchaseOrderRemarkAttachmentAsync(PurchaseOrderRemarkAttachment purchaseOrderRemarkAttachment)
        {
            _context.PurchaseOrderRemarkAttachments.Add(purchaseOrderRemarkAttachment);
            await _context.SaveChangesAsync();
            return purchaseOrderRemarkAttachment;
        }

        public async Task<PurchaseOrderRemarkAttachment> UpdatePurchaseOrderRemarkAttachmentAsync(PurchaseOrderRemarkAttachment purchaseOrderRemarkAttachment)
        {
            _context.PurchaseOrderRemarkAttachments.Update(purchaseOrderRemarkAttachment);
            await _context.SaveChangesAsync();
            return purchaseOrderRemarkAttachment;
        }

        public async Task<bool> DeletePurchaseOrderRemarkAttachmentAsync(long id)
        {
            var purchaseOrderRemarkAttachment = await _context.PurchaseOrderRemarkAttachments.FindAsync(id);
            if (purchaseOrderRemarkAttachment == null)
            {
                return false;
            }

            _context.PurchaseOrderRemarkAttachments.Remove(purchaseOrderRemarkAttachment);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PurchaseOrderTaxes
        public async Task<List<PurchaseOrderTaxis>> GetAllPurchaseOrderTaxesAsync()
        {
            return await _context.PurchaseOrderTaxes.ToListAsync();
        }
        public async Task<List<PurchaseOrderTaxis>> GetAllPurchaseOrderTaxesByPOIdAsync(long poId)
        {
            return await _context.PurchaseOrderTaxes.Where(x=>x.PurchaseOrderId==poId).ToListAsync();
        }

        public async Task<PurchaseOrderTaxis> GetPurchaseOrderTaxesByIdAsync(long id)
        {
            return await _context.PurchaseOrderTaxes.FindAsync(id);
        }

        public async Task<PurchaseOrderTaxis> CreatePurchaseOrderTaxesAsync(PurchaseOrderTaxis purchaseOrderTaxes)
        {
            _context.PurchaseOrderTaxes.Add(purchaseOrderTaxes);
            await _context.SaveChangesAsync();
            return purchaseOrderTaxes;
        }

        public async Task<PurchaseOrderTaxis> UpdatePurchaseOrderTaxesAsync(PurchaseOrderTaxis purchaseOrderTaxes)
        {
            _context.PurchaseOrderTaxes.Update(purchaseOrderTaxes);
            await _context.SaveChangesAsync();
            return purchaseOrderTaxes;
        }

        public async Task<bool> DeletePurchaseOrderTaxesAsync(long id)
        {
            var purchaseOrderTaxes = await _context.PurchaseOrderTaxes.FindAsync(id);
            if (purchaseOrderTaxes == null)
            {
                return false;
            }

            _context.PurchaseOrderTaxes.Remove(purchaseOrderTaxes);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PurchaseOrderAttachments
        public async Task<List<PurchaseOrderAttachment>> GetAllPurchaseOrderAttachmentsAsync()
        {
            return await _context.PurchaseOrderAttachments.ToListAsync();
        }
        public async Task<List<PurchaseOrderAttachment>> GetAllPurchaseOrderAttachmentsByPOIdAsync(long poId)
        {
            return await _context.PurchaseOrderAttachments.Where(x=>x.PurchaseOrderId==poId).ToListAsync();
        }

        public async Task<PurchaseOrderAttachment> GetPurchaseOrderAttachmentsByIdAsync(long id)
        {
            return await _context.PurchaseOrderAttachments.FindAsync(id);
        }

        public async Task<PurchaseOrderAttachment> CreatePurchaseOrderAttachmentsAsync(PurchaseOrderAttachment purchaseOrderAttachments)
        {
            _context.PurchaseOrderAttachments.Add(purchaseOrderAttachments);
            await _context.SaveChangesAsync();
            return purchaseOrderAttachments;
        }

        public async Task<PurchaseOrderAttachment> UpdatePurchaseOrderAttachmentsAsync(PurchaseOrderAttachment purchaseOrderAttachments)
        {
            _context.PurchaseOrderAttachments.Update(purchaseOrderAttachments);
            await _context.SaveChangesAsync();
            return purchaseOrderAttachments;
        }

        public async Task<bool> DeletePurchaseOrderAttachmentsAsync(long id)
        {
            var purchaseOrderAttachments = await _context.PurchaseOrderAttachments.FindAsync(id);
            if (purchaseOrderAttachments == null)
            {
                return false;
            }

            _context.PurchaseOrderAttachments.Remove(purchaseOrderAttachments);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PurchaseTermsAndCondition
        public async Task<List<PurchaseTermsAndCondition>> GetAllPurchaseTermsAndConditionsAsync()
        {
            return await _context.PurchaseTermsAndConditions.ToListAsync();
        }
        public async Task<List<PurchaseTermsAndCondition>> GetAllPurchaseTermsAndConditionsByPOIdAsync(long poId)
        {
            return await _context.PurchaseTermsAndConditions.Where(x=>x.PurchaseOrderId==poId).ToListAsync();
        }

        public async Task<PurchaseTermsAndCondition> GetPurchaseTermsAndConditionByIdAsync(long id)
        {
            return await _context.PurchaseTermsAndConditions.FindAsync(id);
        }

        public async Task<PurchaseTermsAndCondition> CreatePurchaseTermsAndConditionAsync(PurchaseTermsAndCondition purchaseTermsAndCondition)
        {
            _context.PurchaseTermsAndConditions.Add(purchaseTermsAndCondition);
            await _context.SaveChangesAsync();
            return purchaseTermsAndCondition;
        }

        public async Task<PurchaseTermsAndCondition> UpdatePurchaseTermsAndConditionAsync(PurchaseTermsAndCondition purchaseTermsAndCondition)
        {
            _context.PurchaseTermsAndConditions.Update(purchaseTermsAndCondition);
            await _context.SaveChangesAsync();
            return purchaseTermsAndCondition;
        }

        public async Task<bool> DeletePurchaseTermsAndConditionAsync(long id)
        {
            var purchaseTermsAndCondition = await _context.PurchaseTermsAndConditions.FindAsync(id);
            if (purchaseTermsAndCondition == null)
            {
                return false;
            }

            _context.PurchaseTermsAndConditions.Remove(purchaseTermsAndCondition);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PurchaseOrderPayment
        public async Task<List<PurchaseOrderPayment>> GetAllPurchaseOrderPaymentsAsync()
        {
            return await _context.PurchaseOrderPayments.ToListAsync();
        }
        public async Task<List<PurchaseOrderPayment>> GetAllPurchaseOrderPaymentsByPOIdAsync(long poId)
        {
            return await _context.PurchaseOrderPayments.ToListAsync();
        }

        public async Task<PurchaseOrderPayment> GetPurchaseOrderPaymentByIdAsync(long id)
        {
            return await _context.PurchaseOrderPayments.FindAsync(id);
        }

        public async Task<PurchaseOrderPayment> CreatePurchaseOrderPaymentAsync(PurchaseOrderPayment purchaseOrderPayment)
        {
            _context.PurchaseOrderPayments.Add(purchaseOrderPayment);
            await _context.SaveChangesAsync();
            return purchaseOrderPayment;
        }

        public async Task<PurchaseOrderPayment> UpdatePurchaseOrderPaymentAsync(PurchaseOrderPayment purchaseOrderPayment)
        {
            _context.PurchaseOrderPayments.Update(purchaseOrderPayment);
            await _context.SaveChangesAsync();
            return purchaseOrderPayment;
        }

        public async Task<bool> DeletePurchaseOrderPaymentAsync(long id)
        {
            var purchaseOrderPayment = await _context.PurchaseOrderPayments.FindAsync(id);
            if (purchaseOrderPayment == null)
            {
                return false;
            }

            _context.PurchaseOrderPayments.Remove(purchaseOrderPayment);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PurchaseOrderPreApprovedPayment
        public async Task<List<PurchaseOrderPreApprovedPayment>> GetAllPOPreapprvedPaymentsByPOIdAsync(long poId)
        {
            return await _context.PurchaseOrderPreApprovedPayments.Where(x=>x.PurchaseOrderId==poId).ToListAsync();
        }

        public async Task<PurchaseOrderPreApprovedPayment> GetPaymentPreApprovedByIdAsync(long id)
        {
            return await _context.PurchaseOrderPreApprovedPayments.FindAsync(id);
        }

        public async Task<PurchaseOrderPreApprovedPayment> CreatePaymentPreApprovedAsync(PurchaseOrderPreApprovedPayment purchaseOrderPayment)
        {
            _context.PurchaseOrderPreApprovedPayments.Add(purchaseOrderPayment);
            await _context.SaveChangesAsync();
            return purchaseOrderPayment;
        }

        public async Task<PurchaseOrderPreApprovedPayment> UpdatePaymentPreApprovedAsync(PurchaseOrderPreApprovedPayment purchaseOrderPayment)
        {
            _context.PurchaseOrderPreApprovedPayments.Update(purchaseOrderPayment);
            await _context.SaveChangesAsync();
            return purchaseOrderPayment;
        }

        // CRUD Operations for PaymentRemark
        public async Task<List<PaymentRemark>> GetAllPaymentRemarksAsync()
        {
            return await _context.PaymentRemarks.ToListAsync();
        }
        public async Task<List<PaymentRemark>> GetAllPaymentRemarksByPayIdAsync(long payId)
        {
            return await _context.PaymentRemarks.Where(x=>x.PaymentId==payId).ToListAsync();
        }

        public async Task<PaymentRemark> GetPaymentRemarkByIdAsync(long id)
        {
            return await _context.PaymentRemarks.FindAsync(id);
        }

        public async Task<PaymentRemark> CreatePaymentRemarkAsync(PaymentRemark paymentRemark)
        {
            _context.PaymentRemarks.Add(paymentRemark);
            await _context.SaveChangesAsync();
            return paymentRemark;
        }

        public async Task<PaymentRemark> UpdatePaymentRemarkAsync(PaymentRemark paymentRemark)
        {
            _context.PaymentRemarks.Update(paymentRemark);
            await _context.SaveChangesAsync();
            return paymentRemark;
        }

        public async Task<bool> DeletePaymentRemarkAsync(long id)
        {
            var paymentRemark = await _context.PaymentRemarks.FindAsync(id);
            if (paymentRemark == null)
            {
                return false;
            }

            _context.PaymentRemarks.Remove(paymentRemark);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PaymentRemarkAttachment
        public async Task<List<PaymentRemarkAttachment>> GetAllPaymentremarkattachmentsAsync()
        {
            return await _context.PaymentRemarkAttachments.ToListAsync();
        }
        public async Task<List<PaymentRemarkAttachment>> GetAllPaymentRemarkAttachmentsByRemarkIdAsync(long remarkId)
        {
            return await _context.PaymentRemarkAttachments.Where(x=>x.RemarkId==remarkId).ToListAsync();
        }

        public async Task<PaymentRemarkAttachment> GetPaymentRemarkAttachmentByIdAsync(long id)
        {
            return await _context.PaymentRemarkAttachments.FindAsync(id);
        }

        public async Task<PaymentRemarkAttachment> CreatePaymentRemarkAttachmentAsync(PaymentRemarkAttachment paymentRemarkAttachment)
        {
            _context.PaymentRemarkAttachments.Add(paymentRemarkAttachment);
            await _context.SaveChangesAsync();
            return paymentRemarkAttachment;
        }

        public async Task<PaymentRemarkAttachment> UpdatePaymentRemarkAttachmentAsync(PaymentRemarkAttachment paymentRemarkAttachment)
        {
            _context.PaymentRemarkAttachments.Update(paymentRemarkAttachment);
            await _context.SaveChangesAsync();
            return paymentRemarkAttachment;
        }

        public async Task<bool> DeletePaymentRemarkAttachmentAsync(long id)
        {
            var paymentRemarkAttachment = await _context.PaymentRemarkAttachments.FindAsync(id);
            if (paymentRemarkAttachment == null)
            {
                return false;
            }

            _context.PaymentRemarkAttachments.Remove(paymentRemarkAttachment);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PurchaseOrderLineItems
        public async Task<List<PurchaseOrderLineItem>> GetAllPurchaseOrderLineItemsAsync()
        {
            return await _context.PurchaseOrderLineItems.ToListAsync();
        }
        public async Task<List<PurchaseOrderLineItem>> GetAllPurchaseOrderLineItemsByPOIdAsync(long poId)
        {
            return await _context.PurchaseOrderLineItems.Where(x=>x.PurchaseOrderId==poId).ToListAsync();
        }
        public async Task<List<PurchaseOrderLineItem>> GetLineItemsFromPaymentIdAsync(long id)
        {
            var allItemIds = await _context.PaymentItemRelations.Where(x => x.PaymentId == id).Select(x => x.LineItemId).ToListAsync();
            return await _context.PurchaseOrderLineItems.Where(x => allItemIds.Contains(x.Id)).ToListAsync();
        }

        public async Task<PurchaseOrderLineItem> GetPurchaseOrderLineItemsByIdAsync(long id)
        {
            return await _context.PurchaseOrderLineItems.FindAsync(id);
        }

        public async Task<PurchaseOrderLineItem> CreatePurchaseOrderLineItemsAsync(PurchaseOrderLineItem purchaseOrderLineItems)
        {
            _context.PurchaseOrderLineItems.Add(purchaseOrderLineItems);
            await _context.SaveChangesAsync();
            return purchaseOrderLineItems;
        }

        public async Task<PurchaseOrderLineItem> UpdatePurchaseOrderLineItemsAsync(PurchaseOrderLineItem purchaseOrderLineItems)
        {
            _context.PurchaseOrderLineItems.Update(purchaseOrderLineItems);
            await _context.SaveChangesAsync();
            return purchaseOrderLineItems;
        }

        public async Task<bool> DeletePurchaseOrderLineItemsAsync(long id)
        {
            var purchaseOrderLineItems = await _context.PurchaseOrderLineItems.FindAsync(id);
            if (purchaseOrderLineItems == null)
            {
                return false;
            }

            _context.PurchaseOrderLineItems.Remove(purchaseOrderLineItems);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PaymentItemRelation
        public async Task<List<PaymentItemRelation>> GetAllPaymentItemRelationsAsync()
        {
            return await _context.PaymentItemRelations.ToListAsync();
        }
        public async Task<List<PaymentItemRelation>> GetAllPaymentItemRelationsByPayIdAsync(long payId)
        {
            return await _context.PaymentItemRelations.Where(x=>x.PaymentId==payId).ToListAsync();
        }
        public async Task<List<PaymentItemRelation>> GetAllPaymentItemRelationsByItemIdAsync(long itemId)
        {
            return await _context.PaymentItemRelations.Where(x => x.LineItemId == itemId).ToListAsync();
        }

        public async Task<PaymentItemRelation> GetPaymentItemRelationByIdAsync(long id)
        {
            return await _context.PaymentItemRelations.FindAsync(id);
        }
       

        public async Task<PaymentItemRelation> CreatePaymentItemRelationAsync(PaymentItemRelation paymentItemRelation)
        {
            _context.PaymentItemRelations.Add(paymentItemRelation);
            await _context.SaveChangesAsync();
            return paymentItemRelation;
        }

        public async Task<PaymentItemRelation> UpdatePaymentItemRelationAsync(PaymentItemRelation paymentItemRelation)
        {
            _context.PaymentItemRelations.Update(paymentItemRelation);
            await _context.SaveChangesAsync();
            return paymentItemRelation;
        }

        public async Task<bool> DeletePaymentItemRelationAsync(long id)
        {
            var paymentItemRelation = await _context.PaymentItemRelations.FindAsync(id);
            if (paymentItemRelation == null)
            {
                return false;
            }

            _context.PaymentItemRelations.Remove(paymentItemRelation);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for LineItemRemarks
        public async Task<List<LineItemRemark>> GetAllLineItemRemarksAsync()
        {
            return await _context.LineItemRemarks.ToListAsync();
        }
        public async Task<List<LineItemRemark>> GetAllLineItemRemarksByItemIdAsync(long itemId)
        {
            return await _context.LineItemRemarks.Where(x=>x.LineItemId==itemId).ToListAsync();
        }

        public async Task<LineItemRemark> GetLineItemRemarksByIdAsync(long id)
        {
            return await _context.LineItemRemarks.FindAsync(id);
        }

        public async Task<LineItemRemark> CreateLineItemRemarksAsync(LineItemRemark lineItemRemarks)
        {
            _context.LineItemRemarks.Add(lineItemRemarks);
            await _context.SaveChangesAsync();
            return lineItemRemarks;
        }

        public async Task<LineItemRemark> UpdateLineItemRemarksAsync(LineItemRemark lineItemRemarks)
        {
            _context.LineItemRemarks.Update(lineItemRemarks);
            await _context.SaveChangesAsync();
            return lineItemRemarks;
        }

        public async Task<bool> DeleteLineItemRemarksAsync(long id)
        {
            var lineItemRemarks = await _context.LineItemRemarks.FindAsync(id);
            if (lineItemRemarks == null)
            {
                return false;
            }

            _context.LineItemRemarks.Remove(lineItemRemarks);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for LineItemRemarkAttachments
        public async Task<List<LineItemRemarkAttachment>> GetAllLineItemRemarkAttachmentsAsync()
        {
            return await _context.LineItemRemarkAttachments.ToListAsync();
        }
        public async Task<List<LineItemRemarkAttachment>> GetAllLineItemRemarkAttachmentsByRemarkIdAsync(long remarkId)
        {
            return await _context.LineItemRemarkAttachments.Where(x=>x.RemarkId==remarkId).ToListAsync();
        }

        public async Task<LineItemRemarkAttachment> GetLineItemRemarkAttachmentsByIdAsync(long id)
        {
            return await _context.LineItemRemarkAttachments.FindAsync(id);
        }

        public async Task<LineItemRemarkAttachment> CreateLineItemRemarkAttachmentsAsync(LineItemRemarkAttachment lineItemRemarkAttachments)
        {
            _context.LineItemRemarkAttachments.Add(lineItemRemarkAttachments);
            await _context.SaveChangesAsync();
            return lineItemRemarkAttachments;
        }

        public async Task<LineItemRemarkAttachment> UpdateLineItemRemarkAttachmentsAsync(LineItemRemarkAttachment lineItemRemarkAttachments)
        {
            _context.LineItemRemarkAttachments.Update(lineItemRemarkAttachments);
            await _context.SaveChangesAsync();
            return lineItemRemarkAttachments;
        }

        public async Task<bool> DeleteLineItemRemarkAttachmentsAsync(long id)
        {
            var lineItemRemarkAttachments = await _context.LineItemRemarkAttachments.FindAsync(id);
            if (lineItemRemarkAttachments == null)
            {
                return false;
            }

            _context.LineItemRemarkAttachments.Remove(lineItemRemarkAttachments);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for PurchaseOrderLineItemsAttachments
        public async Task<List<PurchaseOrderLineItemsAttachment>> GetAllPurchaseOrderLineItemsAttachmentsAsync()
        {
            return await _context.PurchaseOrderLineItemsAttachments.ToListAsync();
        }
        public async Task<List<PurchaseOrderLineItemsAttachment>> GetAllPurchaseOrderLineItemsAttachmentsByItemIdAsync(long itemId)
        {
            return await _context.PurchaseOrderLineItemsAttachments.Where(x=>x.LineItemId==itemId).ToListAsync();
        }

        public async Task<PurchaseOrderLineItemsAttachment> GetPurchaseOrderLineItemsAttachmentsByIdAsync(long id)
        {
            return await _context.PurchaseOrderLineItemsAttachments.FindAsync(id);
        }

        public async Task<PurchaseOrderLineItemsAttachment>
            CreatePurchaseOrderLineItemsAttachmentsAsync
            (PurchaseOrderLineItemsAttachment purchaseOrderLineItemsAttachments)
        {
            _context.PurchaseOrderLineItemsAttachments.Add(purchaseOrderLineItemsAttachments);
            await _context.SaveChangesAsync();
            return purchaseOrderLineItemsAttachments;
        }

        public async Task<PurchaseOrderLineItemsAttachment>
            UpdatePurchaseOrderLineItemsAttachmentsAsync
            (PurchaseOrderLineItemsAttachment purchaseOrderLineItemsAttachments)
        {
            _context.PurchaseOrderLineItemsAttachments.Update(purchaseOrderLineItemsAttachments);
            await _context.SaveChangesAsync();
            return purchaseOrderLineItemsAttachments;
        }

        public async Task<bool> DeletePurchaseOrderLineItemsAttachmentsAsync(long id)
        {
            var purchaseOrderLineItemsAttachments = await _context.PurchaseOrderLineItemsAttachments.FindAsync(id);
            if (purchaseOrderLineItemsAttachments == null)
            {
                return false;
            }

            _context.PurchaseOrderLineItemsAttachments.Remove(purchaseOrderLineItemsAttachments);
            await _context.SaveChangesAsync();
            return true;
        }

        // CRUD Operations for UserActivityLog
        public async Task<List<UserActivityLog>> GetAllUserActivityLogsAsync()
        {
            return await _context.UserActivityLogs.ToListAsync();
        }

        public async Task<UserActivityLog> GetUserActivityLogByIdAsync(long id)
        {
            return await _context.UserActivityLogs.FindAsync(id);
        }

        public async Task<UserActivityLog> CreateUserActivityLogAsync(UserActivityLog userActivityLog)
        {
            _context.UserActivityLogs.Add(userActivityLog);
            await _context.SaveChangesAsync();
            return userActivityLog;
        }

        public async Task<UserActivityLog> UpdateUserActivityLogAsync(UserActivityLog userActivityLog)
        {
            _context.UserActivityLogs.Update(userActivityLog);
            await _context.SaveChangesAsync();
            return userActivityLog;
        }

        public async Task<bool> DeleteUserActivityLogAsync(long id)
        {
            var userActivityLog = await _context.UserActivityLogs.FindAsync(id);
            if (userActivityLog == null)
            {
                return false;
            }

            _context.UserActivityLogs.Remove(userActivityLog);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
