using BusinessLogic.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using POManagementAPI.Helper;
using POManagementAPI.Models;
using POManagementAPI.Services;

namespace POManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class POManagementController : ControllerBase
    {
        private readonly IPOManagerService _service;

        public POManagementController(IPOManagerService service)
        {
            _service = service;
        }

        private Client getTokenData()
        {
            var tokenData = new Client();
            if (this.HttpContext.Items.ContainsKey("User"))
            {
                tokenData = this.HttpContext.Items["User"] as Client;
            }
            return tokenData;

        }
        //[HttpPost("AddUserActivity")]
        //public async Task<IActionResult> AddUserActivity([FromBody] UserActivityRequest request)
        //{
        //    await _service.AddUserActivityAsync(request.Description, request.PoId, request.UId, request.Type);
        //    return Ok();
        //}
        [POManagerAuthorize]
        [HttpGet("GetUserOrAddUser")]
        public async Task<IActionResult> GetUserOrAddUser(string phoneNumber)
        {
            var userId = await _service.GetUserOrAddUserAsync(phoneNumber);
            return Ok(userId);
        }
        [POManagerAuthorize]
        [HttpGet("GetPOAssociatedWithUser")]
        public async Task<GenericResponseModel<List<PurchaseOrderShortDisplay>>> GetPOAssociatedWithUser(bool isRaisedBy)
        {
            var userId = await _service.GetPOListRaisedByOrRaisedFor(getTokenData().Id, isRaisedBy);
            return userId;
        }

        [POManagerAuthorize]
        [HttpGet("UserSuggestions")]
        public async Task<GenericResponseModel<List<UserSuggestion>>> GetUserSuggestion(string stringParts)
        {
            return await _service.getUserSuggestions(stringParts);
        }

        [POManagerAuthorize]
        [HttpPost("BlockAndUnblockClient")]
        public async Task<IActionResult> BlockAndUnblockClient(long uid, bool blockStatus)
        {
            var result = await _service.BlockAndUnblockClientAsync(uid, blockStatus);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpGet("GetClient")]
        public async Task<IActionResult> GetClient(long uid)
        {
            var client = await _service.GetClientAsync(uid);
            return Ok(client);
        }

        [POManagerAuthorize]
        [HttpPost("AddOrUpdatePurchaseOrder")]
        public async Task<IActionResult> AddOrUpdatePurchaseOrder([FromBody] PORequest order)
        {
            var result = await _service.AddOrUpdatePurchaseOrderAsync(order);
            await _service.AddUserActivityAsync("Purchase order created or updated", result, getTokenData().Id, 16);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("AddOrUpdateItem")]
        public async Task<IActionResult> AddOrUpdateItem([FromBody] LineItem lineItem)
        {
            var result = await _service.AddOrUpdateItemAsync(lineItem);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("AddPurchaseOrderAttachment")]
        public async Task<IActionResult> AddPurchaseOrderAttachment(long orderId, char attachmentType, string attachmentLink)
        {
            var result = await _service.AddPurchaseOrderAttachmentAsync(orderId, attachmentType, attachmentLink);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("AddLineItemAttachment")]
        public async Task<IActionResult> AddLineItemAttachment(long lineItemId, string attachmentLink)
        {
            var result = await _service.AddLineItemAttachmentAsync(lineItemId, attachmentLink);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("AddRemarks")]
        public async Task<IActionResult> AddRemarks([FromBody] RemarksRequest request)
        {
            var result = await _service.AddRemarksAsync(request.Remark, request.Parent, request.ParentId, request.AttachmentLinks);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("AddTermsAndConditions")]
        public async Task<IActionResult> AddTermsAndConditions([FromBody] TermsAndConditionsRequest request)
        {
            var result = await _service.AddTermsAndConditionsAsync(request.Val, request.Seq, request.PoId);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("UpdateTermsAndConditions")]
        public async Task<IActionResult> UpdateTermsAndConditions([FromBody] TermsAndConditionsRequest request)
        {
            var result = await _service.UpdateTermsAndConditionsAsync(request.TermId, request.Val, request.Seq);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("AddPurchaseOrderTaxes")]
        public async Task<IActionResult> AddPurchaseOrderTaxes([FromBody] TaxesRequest request)
        {
            var result = await _service.AddPurchaseOrderTaxesAsync(request.PoId, request.Percent, request.Title);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("UpdatePurchaseOrderTaxes")]
        public async Task<IActionResult> UpdatePurchaseOrderTaxes([FromBody] TaxesRequest request)
        {
            var result = await _service.UpdatePurchaseOrderTaxesAsync(request.TaxId, request.Percent, request.Title);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("AddPreApprovalPaymentFrequencyAndPartBased")]
        public async Task<IActionResult> AddPreApprovalPaymentFrequencyAndPartBased([FromBody] PreApprovalPaymentFrequencyAndPartBasedRequest request)
        {
            var result = await _service.AddPreApprovalPaymentFrequencyAndPartBasedAsync(request.PoId, request.Seq, request.Note, request.Amt, request.Frq, request.PaymentType);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("AddPreApprovalPaymentItemBased")]
        public async Task<IActionResult> AddPreApprovalPaymentItemBased([FromBody] PreApprovalPaymentItemBasedRequest request)
        {
            var result = await _service.AddPreApprovalPaymentItemBasedAsync(request.PoId, request.Seq, request.Note, request.Amt, request.ItemIds);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("EditPreApprovalPaymentFrequencyAndPartBased")]
        public async Task<IActionResult> EditPreApprovalPaymentFrequencyAndPartBased([FromBody] PreApprovalPaymentFrequencyAndPartBasedRequest request)
        {
            var result = await _service.EditPreApprovalPaymentFrequencyAndPartBasedAsync(request.PayId, request.PoId, request.Seq, request.Note, request.Amt, request.Frq, request.PaymentType);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("EditPreApprovalPaymentItemBased")]
        public async Task<IActionResult> EditPreApprovalPaymentItemBased([FromBody] PreApprovalPaymentItemBasedRequest request)
        {
            var result = await _service.EditPreApprovalPaymentItemBasedAsync(request.PayId, request.PoId, request.Seq, request.Note, request.Amt, request.ItemIds);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("DeclinePurchaseOrder")]
        public async Task<IActionResult> DeclinePurchaseOrder(long poId)
        {
            var result = await _service.DeclinePurchaseOrderAsync(poId);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("ReconsiderPurchaseOrder")]
        public async Task<IActionResult> ReconsiderPurchaseOrder(long poId)
        {
            var result = await _service.ReconsiderPurchaseOrderAsync(poId);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("RaisePurchaseOrder")]
        public async Task<IActionResult> RaisePurchaseOrder(long poId)
        {
            var result = await _service.RaisePurchaseOrderAsync(poId);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("AcceptPurchaseOrder")]
        public async Task<IActionResult> AcceptPurchaseOrder(long poId)
        {
            var result = await _service.AcceptPurchaseOrderAsync(poId);
            await _service.AddUserActivityAsync("Purchasem order status is changed to Approved", result, getTokenData().Id, 0);
            return Ok(result);
        }
        [POManagerAuthorize]
        [HttpPost("CompletePurchaseOrder")]
        public async Task<IActionResult> CompletePurchaseOrder(long poId)
        {
            var result = await _service.CompletePurchaseOrderAsync(poId);
            return Ok(result);
        }
    }
    public class PORequest {
        public long Id { get; set; }
        public long PoRaisedBy { get; set; }

        public string PoRaisedForPhoneNumber { get; set; }

        public string PoTitle { get; set; } = null!;

        public string PoDescription { get; set; } = null!;

        public decimal PoNotificationPeriod { get; set; }

        public int PoCompletionDurationInDays { get; set; }
        public DateTime PoStartDate { get; set; }

        public decimal PoTotalAmount { get; set; }

        public decimal PoDiscount { get; set; }

    }
    public class UserActivityRequest
    {
        public string Description { get; set; }
        public long PoId { get; set; }
        public long UId { get; set; }
        public sbyte Type { get; set; }
    }

    public class OTPRequest
    {
        public string PhoneNumber { get; set; }
        public string OTP { get; set; }
    }

    public class RemarksRequest
    {
        public string Remark { get; set; }
        public char Parent { get; set; }
        public long ParentId { get; set; }
        public List<string> AttachmentLinks { get; set; }
    }

    public class TermsAndConditionsRequest
    {
        public long TermId { get; set; }
        public string Val { get; set; }
        public int Seq { get; set; }
        public long PoId { get; set; }
    }

    public class TaxesRequest
    {
        public long TaxId { get; set; }
        public decimal Percent { get; set; }
        public string Title { get; set; }
        public long PoId { get; set; }
    }

    public class PreApprovalPaymentFrequencyAndPartBasedRequest
    {
        public long PayId { get; set; }
        public long PoId { get; set; }
        public int Seq { get; set; }
        public string Note { get; set; }
        public decimal Amt { get; set; }
        public int Frq { get; set; }
        public char PaymentType { get; set; }
    }

    public class PreApprovalPaymentItemBasedRequest
    {
        public long PayId { get; set; }
        public long PoId { get; set; }
        public int Seq { get; set; }
        public string Note { get; set; }
        public decimal Amt { get; set; }
        public List<long> ItemIds { get; set; }
    }
}
