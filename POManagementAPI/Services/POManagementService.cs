using BusinessLogic;
using BusinessLogic.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using POManagementAPI.Controllers;
using POManagementAPI.Models;
using POManagementDataAccessLayer.DataAccessLayer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace POManagementAPI.Services
{
    public interface IPOManagerService {
        Task<long> GetUserOrAddUserAsync(string phoneNumber);
        Task<Client> GetClientAsync(long uid);
        Task<LoginResponseModel> login(OTPRequest request);
        Task<bool> NotifyUser(string phoneNo);
        Task<bool> UpdateClientAsync(Client client);
        Task AddUserActivityAsync(string description, long poId, long uId, sbyte type);
        Task<bool> ValidateOTPAsync(long uid, string otp);
        Task<bool> LinkOTPValidationAsync(long uid, string otp);
        Task<bool> BlockAndUnblockClientAsync(long uid, bool blockStatus);
        Task<long> AddOrUpdatePurchaseOrderAsync(PORequest order);
        Task<long> AddOrUpdateItemAsync(LineItem lineItem);
        Task<long> AddPurchaseOrderAttachmentAsync(long orderId, char attachmentType, string attachmentLink);
        Task<long> AddLineItemAttachmentAsync(long lineItemId, string attachmentLink);
        Task<long> AddRemarksAsync(string remark, char parent, long parentId, List<string> attachmentLinks);
        Task<long> AddTermsAndConditionsAsync(string val, int seq, long poId);
        Task<long> UpdateTermsAndConditionsAsync(long termId, string val, int seq);
        Task<long> AddPurchaseOrderTaxesAsync(long poId, decimal percent, string title);
        Task<long> UpdatePurchaseOrderTaxesAsync(long taxId, decimal percent, string title);
        Task<long> AddPreApprovalPaymentFrequencyAndPartBasedAsync(long poId, int seq, string note, decimal amt, int frq, char paymentType);
        Task<long> AddPreApprovalPaymentItemBasedAsync(long poId, int seq, string note, decimal amt, List<long> itemIds);
        Task<long> EditPreApprovalPaymentFrequencyAndPartBasedAsync(long payId, long poId, int seq, string note, decimal amt, int frq, char paymentType);
        Task<long> EditPreApprovalPaymentItemBasedAsync(long payId, long poId, int seq, string note, decimal amt, List<long> itemIds);
        Task<GenericResponseModel<List<UserSuggestion>>> getUserSuggestions(string stringParts);
        Task<long> DeclinePurchaseOrderAsync(long poId);
        Task<long> ReconsiderPurchaseOrderAsync(long poId);
        Task<long> RaisePurchaseOrderAsync(long poId);
        Task<long> CompletePurchaseOrderAsync(long poId);
        Task<long> DeliveryDoneOrPayUndonePurchaseOrderAsync(long poId);
        Task<long> DeliveryUndonePurchaseOrderAsync(long poId);
        Task<long> DeliveryReceivedPurchaseOrderAsync(long poId);
        Task<long> AcceptPurchaseOrderAsync(long poId);
        Task<long> AskForPayAndPayNotDonePaymentStatusChange(long payId);
        Task<long> InvalidPayAskPaymentStatusChange(long payId);
        Task<long> ClaimPayPaymentStatusChange(long payId);
        Task<long> PayClaimAcceptedPaymentStatusChange(long payId);
        Task<long> DeliveryDoneClaimPayLineItemStatusChange(long id);
        Task<long> DeliveryUndoneUnclaimPayLineItemStatusChange(long id);
        Task<long> CompletePayClaimedLineItemStatusChange(long id);
        Task<GenericResponseModel<PurchaseOrderDetailedDisplay>> GetCompletePO(long poId);
        Task<GenericResponseModel<List<PurchaseOrderShortDisplay>>> GetPOListRaisedByOrRaisedFor(long usrId, bool isRaisedBy);
    }
    public class POManagementService : IPOManagerService
    {
        private readonly POManagementBL _bl;
        private readonly AppSettings _appSettings;

        public POManagementService(DbContextOptions<poMngtSQLContext> options, IOptions<AppSettings> appSettings)
        {
            _bl = new POManagementBL(options);
            _appSettings = appSettings.Value;
        }

        private string generateToken(string id, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ContractAPIConstants.AppClaimName, id) }),
                Expires = DateTime.UtcNow.AddHours(_appSettings.TokenExpiryInHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<LoginResponseModel> login(OTPRequest request) 
        {
            var result = new LoginResponseModel() { Message="Invalid Info"};
            var uId=await _bl.GetUserIdByPhoneNumber(request.PhoneNumber);
            var status =await ValidateOTPAsync(uId, request.OTP);
            if (status) 
            {
                result.Status = GeneralResponseStatus.SUCCESS;
                result.Message = "Valid client";
                result.Token = generateToken(uId.ToString(), _appSettings.AppSecret);
            }
            return result;
        }

        public async Task AddUserActivityAsync(string description, long poId, long uId, sbyte type)
        {
            await _bl.AddUserActivity(description, poId, uId, type);
        }

        public async Task<long> GetUserOrAddUserAsync(string phoneNumber)
        {
            return await _bl.GetUserOrAddUser(phoneNumber);
        }
        public async Task<bool> NotifyUser(string phoneNo)
        {
            var uId=await _bl.GetUserIdByPhoneNumber(phoneNo);
            if (uId == 0) 
            {
                uId=await _bl.GetUserOrAddUser(phoneNo);
            }
            var generatedOTP= await _bl.AddNewOTP(uId);
            //Send this OTP to User Via SMS, Watsapp and Email
            return generatedOTP.Length==6;
        }

        public async Task<bool> ValidateOTPAsync(long uid, string otp)
        {
            //Link will become invlid if you try to login before clicking on link and link is generated 20 mins before.
            return await _bl.ValidateOTP(uid, otp);
        }
        public async Task<bool> LinkOTPValidationAsync(long uid, string otp)
        {
            //Link will become invlid if you try to login before clicking on link and link is generated 20 mins before.
            return await _bl.ValidateOTP(uid, otp,false);
        }

        public async Task<bool> UpdateClientAsync(Client client)
        {
            return await _bl.UpdateClient(client);
        }

        public async Task<bool> BlockAndUnblockClientAsync(long uid, bool blockStatus)
        {
            return await _bl.BlockAndUnblockClient(uid, blockStatus);
        }

        public async Task<Client> GetClientAsync(long uid)
        {
            return await _bl.GetClient(uid);
        }


        public async Task<long> AddOrUpdatePurchaseOrderAsync(PORequest order)
        {
            var uId=await _bl.GetUserIdByPhoneNumber(order.PoRaisedForPhoneNumber);
            return await _bl.AddOrUpdatePurchaseOrder(new PO() {
                PoTotalAmount=order.PoTotalAmount,
                ModifiedOn=DateTime.Now,
                PoCompletionDurationInDays=order.PoCompletionDurationInDays,
                PoDescription=order.PoDescription,
                PoDiscount=order.PoDiscount,
                PoNotificationPeriod=order.PoNotificationPeriod,
                PoRaisedBy=order.PoRaisedBy,
                PoRaisedFor=uId,
                Id=order.Id
            });
        }
        public async Task <GenericResponseModel< List<UserSuggestion>>> getUserSuggestions(string stringParts) 
        {
            return new GenericResponseModel<List<UserSuggestion>>(await _bl.GetUserSuggestions(stringParts));
        }

        public async Task<long> AddOrUpdateItemAsync(LineItem lineItem)
        {
            return await _bl.AddOrUpdateItem(lineItem);
        }

        public async Task<long> AddPurchaseOrderAttachmentAsync(long orderId, char attachmentType, string attachmentLink)
        {
            return await _bl.AddPurchaseOrderAttachment(orderId, attachmentType, attachmentLink);
        }

        public async Task<long> AddLineItemAttachmentAsync(long lineItemId, string attachmentLink)
        {
            return await _bl.AddLineItemAttachment(lineItemId, attachmentLink);
        }

        public async Task<long> AddRemarksAsync(string remark, char parent, long parentId, List<string> attachmentLinks)
        {
            return await _bl.AddRemarks(remark, parent, parentId, attachmentLinks);
        }

        public async Task<long> AddTermsAndConditionsAsync(string val, int seq, long poId)
        {
            return await _bl.AddTermsAndConditions(val, seq, poId);
        }

        public async Task<long> UpdateTermsAndConditionsAsync(long termId, string val, int seq)
        {
            return await _bl.UpdateTermsAndConditions(termId, val, seq);
        }

        public async Task<long> AddPurchaseOrderTaxesAsync(long poId, decimal percent, string title)
        {
            return await _bl.AddPurchaseOrderTaxes(poId, percent, title);
        }

        public async Task<long> UpdatePurchaseOrderTaxesAsync(long taxId, decimal percent, string title)
        {
            return await _bl.UpdatePurchaseOrderTaxes(taxId, percent, title);
        }

        public async Task<long> AddPreApprovalPaymentFrequencyAndPartBasedAsync(long poId, int seq, string note, decimal amt, int frq, char paymentType)
        {
            return await _bl.AddPreApprovalPaymentFrequencyAndPartBased(poId, seq, note, amt, frq, paymentType);
        }

        public async Task<long> AddPreApprovalPaymentItemBasedAsync(long poId, int seq, string note, decimal amt, List<long> itemIds)
        {
            return await _bl.AddPreApprovalPaymentItemBased(poId, seq, note, amt, itemIds);
        }

        public async Task<long> EditPreApprovalPaymentFrequencyAndPartBasedAsync(long payId, long poId, int seq, string note, decimal amt, int frq, char paymentType)
        {
            return await _bl.EditPreApprovalPaymentFrequencyAndPartBased(payId, poId, seq, note, amt, frq, paymentType);
        }

        public async Task<long> EditPreApprovalPaymentItemBasedAsync(long payId, long poId, int seq, string note, decimal amt, List<long> itemIds)
        {
            return await _bl.EditPreApprovalPaymentItemBased(payId, poId, seq, note, amt, itemIds);
        }
        //----------------------Item--------------------------------
        public async Task<long> DeliveryDoneClaimPayLineItemStatusChange(long id)
        {
            return await _bl.DeliveryDoneClaimPayLineItemStatusChange(id);

        }
        public async Task<long> DeliveryUndoneUnclaimPayLineItemStatusChange(long id)
        {
            return await _bl.DeliveryUndoneUnclaimPayLineItemStatusChange(id);
        }
        public async Task<long> CompletePayClaimedLineItemStatusChange(long id)
        {
            return await _bl.CompletePayClaimedLineItemStatusChange(id);
        }
        //------------------------------------------------------
        //----------------------Payment--------------------------------
        public async Task<long> AskForPayAndPayNotDonePaymentStatusChange(long payId)
        {
            return await _bl.AskForPayAndPayNotDonePaymentStatusChange(payId);
        }
        public async Task<long> InvalidPayAskPaymentStatusChange(long payId)
        {
            return await _bl.InvalidPayAskPaymentStatusChange(payId);
        }
        public async Task<long> ClaimPayPaymentStatusChange(long payId)
        {
            return await _bl.ClaimPayPaymentStatusChange(payId);
        }
        public async Task<long> PayClaimAcceptedPaymentStatusChange(long payId)
        {
            return await _bl.PayClaimAcceptedPaymentStatusChange(payId);
        }
        //------------------------------------------------------

        //----------------------PO--------------------------------
        public async Task<long> DeclinePurchaseOrderAsync(long poId)
        {
            return await _bl.DeclinePurchaseOrderAsync(poId);
        }
        public async Task<long> ReconsiderPurchaseOrderAsync(long poId)
        {
            return await _bl.ReconsiderPurchaseOrderAsync(poId);
        }
        public async Task<long> RaisePurchaseOrderAsync(long poId)
        {
            return await _bl.RaisePurchaseOrderAsync(poId);
        }
        public async Task<long> CompletePurchaseOrderAsync(long poId) 
        {
            return await _bl.CompletePurchaseOrder(poId);
        }
        public async Task<long> DeliveryDoneOrPayUndonePurchaseOrderAsync(long poId)
        {
            return await _bl.DeliveryDoneOrPayUndonePurchaseOrderAsync(poId);
        }
        public async Task<long> DeliveryUndonePurchaseOrderAsync(long poId)
        {
            return await _bl.DeliveryUndonePurchaseOrderAsync(poId);
        }
        public async Task<long> DeliveryReceivedPurchaseOrderAsync(long poId)
        {
            return await _bl.DeliveryReceivedPurchaseOrderAsync(poId);
        }
        public async Task<long> AcceptPurchaseOrderAsync(long poId)
        {
            return await _bl.AcceptedPurchaseOrder(poId);
        }

        //------------------------------------------------------

        public async Task<GenericResponseModel<PurchaseOrderDetailedDisplay>> GetCompletePO(long poId) 
        {
            var o = await _bl.GetPurchaseOrderDetailedDisplay(poId);
            if (o == null) 
            {
                return new GenericResponseModel<PurchaseOrderDetailedDisplay>(null) { Status = GeneralResponseStatus.FAILED, Message = "Object Not Found" };
            }
            return new GenericResponseModel<PurchaseOrderDetailedDisplay>(o) { Status=GeneralResponseStatus.SUCCESS,Message="Object Found"};
        }
        public async Task<GenericResponseModel<List<PurchaseOrderShortDisplay>>> GetPOListRaisedByOrRaisedFor(long usrId, bool isRaisedBy) 
        {
            var o = new List<PurchaseOrderShortDisplay>();
            if (isRaisedBy)
            {
                o = await _bl.GetPurchaseOrderRaisedBy(usrId);
            }
            else {
                o = await _bl.GetPurchaseOrderRaisedFor(usrId);
            }
            if (o == null || o.Count == 0) 
            {
                return new GenericResponseModel<List<PurchaseOrderShortDisplay>>(o) { Message = "Unable to find any purchase order", Status = GeneralResponseStatus.FAILED }; 
            }
            return new GenericResponseModel<List<PurchaseOrderShortDisplay>>(o) { Message = "Orders found", Status = GeneralResponseStatus.SUCCESS };
        }

    }
}
