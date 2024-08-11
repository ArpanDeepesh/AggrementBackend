using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace POManagementDataAccessLayer.DataAccessLayer;

public partial class poMngtSQLContext : DbContext
{
    public poMngtSQLContext()
    {
    }

    public poMngtSQLContext(DbContextOptions<poMngtSQLContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LineItemRemark> LineItemRemarks { get; set; }

    public virtual DbSet<LineItemRemarkAttachment> LineItemRemarkAttachments { get; set; }

    public virtual DbSet<PaymentItemRelation> PaymentItemRelations { get; set; }

    public virtual DbSet<PaymentRemark> PaymentRemarks { get; set; }

    public virtual DbSet<PaymentRemarkAttachment> PaymentRemarkAttachments { get; set; }

    public virtual DbSet<PoAndItemStatus> PoAndItemStatuses { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderAttachment> PurchaseOrderAttachments { get; set; }

    public virtual DbSet<PurchaseOrderLineItem> PurchaseOrderLineItems { get; set; }

    public virtual DbSet<PurchaseOrderLineItemsAttachment> PurchaseOrderLineItemsAttachments { get; set; }

    public virtual DbSet<PurchaseOrderPayment> PurchaseOrderPayments { get; set; }

    public virtual DbSet<PurchaseOrderPreApprovedPayment> PurchaseOrderPreApprovedPayments { get; set; }

    public virtual DbSet<PurchaseOrderRemark> PurchaseOrderRemarks { get; set; }

    public virtual DbSet<PurchaseOrderRemarkAttachment> PurchaseOrderRemarkAttachments { get; set; }

    public virtual DbSet<PurchaseOrderTaxis> PurchaseOrderTaxes { get; set; }

    public virtual DbSet<PurchaseTermsAndCondition> PurchaseTermsAndConditions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserActivityLog> UserActivityLogs { get; set; }

    public virtual DbSet<UserActivityType> UserActivityTypes { get; set; }

    public virtual DbSet<UsersOtp> UsersOtps { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("Host=127.0.0.1;Database=podb;Username=root;Password=");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LineItemRemark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("line_item_remarks");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.LineItemId).HasColumnName("line_item_id");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.RemarkTxt)
                .HasMaxLength(1000)
                .HasColumnName("remark_txt");
        });

        modelBuilder.Entity<LineItemRemarkAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("line_item_remark_attachments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttachmentLink)
                .HasMaxLength(500)
                .HasColumnName("attachment_link");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.RemarkId).HasColumnName("remark_id");
        });

        modelBuilder.Entity<PaymentItemRelation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payment_item_relation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.LineItemId).HasColumnName("line_item_id");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
        });

        modelBuilder.Entity<PaymentRemark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payment_remark");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.RemarkTxt)
                .HasMaxLength(1000)
                .HasColumnName("remark_txt");
        });

        modelBuilder.Entity<PaymentRemarkAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payment_remark_attachment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttachmentLink)
                .HasMaxLength(500)
                .HasColumnName("attachment_link");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.RemarkId).HasColumnName("remark_id");
        });

        modelBuilder.Entity<PoAndItemStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("po_and_item_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StatusValue)
                .HasMaxLength(20)
                .HasColumnName("status_value");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_order");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PoDescription)
                .HasMaxLength(1000)
                .HasColumnName("po_description");
            entity.Property(e => e.PoDiscount)
                .HasPrecision(5)
                .HasColumnName("po_discount");
            entity.Property(e => e.PoDurationInDays).HasColumnName("po_duration_in_days");
            entity.Property(e => e.PoNotificationPeriodInDays)
                .HasPrecision(2)
                .HasColumnName("po_notification_period_in_days");
            entity.Property(e => e.PoRaisedBy).HasColumnName("po_raised_by");
            entity.Property(e => e.PoRaisedFor).HasColumnName("po_raised_for");
            entity.Property(e => e.PoStartDate)
                .HasColumnType("datetime")
                .HasColumnName("po_start_date");
            entity.Property(e => e.PoStatus).HasColumnName("po_status");
            entity.Property(e => e.PoTitle)
                .HasMaxLength(100)
                .HasColumnName("po_title");
            entity.Property(e => e.PoTotalAmount)
                .HasPrecision(10)
                .HasColumnName("po_total_amount");
        });

        modelBuilder.Entity<PurchaseOrderAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_order_attachments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttachmentLink)
                .HasMaxLength(500)
                .HasColumnName("attachment_link");
            entity.Property(e => e.AttachmentType).HasColumnName("attachment_type");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
        });

        modelBuilder.Entity<PurchaseOrderLineItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_order_line_items");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.LiDescription)
                .HasMaxLength(1000)
                .HasColumnName("li_description");
            entity.Property(e => e.LiItemCompletionDuration).HasColumnName("li_item_completion_duration");
            entity.Property(e => e.LiQuantity)
                .HasPrecision(10)
                .HasColumnName("li_quantity");
            entity.Property(e => e.LiRate)
                .HasPrecision(10)
                .HasColumnName("li_rate");
            entity.Property(e => e.LiTitle)
                .HasMaxLength(100)
                .HasColumnName("li_title");
            entity.Property(e => e.LineItemStatus).HasColumnName("line_item_status");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
        });

        modelBuilder.Entity<PurchaseOrderLineItemsAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_order_line_items_attachments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttachmentLink)
                .HasMaxLength(500)
                .HasColumnName("attachment_link");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.LineItemId).HasColumnName("line_item_id");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
        });

        modelBuilder.Entity<PurchaseOrderPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_order_payment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.DueDate)
                .HasColumnType("datetime")
                .HasColumnName("due_date");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PaymentAmount)
                .HasPrecision(10)
                .HasColumnName("payment_amount");
            entity.Property(e => e.PaymentCompletionCriteria).HasColumnName("payment_completion_criteria");
            entity.Property(e => e.PaymentNotes)
                .HasMaxLength(500)
                .HasColumnName("payment_notes");
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.SequenceNo).HasColumnName("sequence_no");
        });

        modelBuilder.Entity<PurchaseOrderPreApprovedPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_order_pre_approved_payment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ExtraInformation)
                .HasMaxLength(500)
                .HasColumnName("extra_information");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PaymentAmount)
                .HasPrecision(10)
                .HasColumnName("payment_amount");
            entity.Property(e => e.PaymentCompletionCriteria).HasColumnName("payment_completion_criteria");
            entity.Property(e => e.PaymentNotes)
                .HasMaxLength(500)
                .HasColumnName("payment_notes");
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.SequenceNo).HasColumnName("sequence_no");
        });

        modelBuilder.Entity<PurchaseOrderRemark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_order_remark");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.RemarkTxt)
                .HasMaxLength(1000)
                .HasColumnName("remark_txt");
        });

        modelBuilder.Entity<PurchaseOrderRemarkAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_order_remark_attachment");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttachmentLink)
                .HasMaxLength(500)
                .HasColumnName("attachment_link");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.RemarkId).HasColumnName("remark_id");
        });

        modelBuilder.Entity<PurchaseOrderTaxis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_order_taxes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.TaxPercentage)
                .HasPrecision(5)
                .HasColumnName("tax_percentage");
            entity.Property(e => e.TaxTitle)
                .HasMaxLength(50)
                .HasColumnName("tax_title");
        });

        modelBuilder.Entity<PurchaseTermsAndCondition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("purchase_terms_and_condition");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PurchaseOrderId).HasColumnName("purchase_order_id");
            entity.Property(e => e.SequenceNo).HasColumnName("sequence_no");
            entity.Property(e => e.TermsAndConditionsValue)
                .HasMaxLength(1000)
                .HasColumnName("terms_and_conditions_value");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateOn)
                .HasColumnType("datetime")
                .HasColumnName("create_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(50)
                .HasColumnName("user_email");
            entity.Property(e => e.UserGstin)
                .HasMaxLength(50)
                .HasColumnName("user_GSTIN");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");
            entity.Property(e => e.UserSmsContact)
                .HasMaxLength(15)
                .HasColumnName("user_sms_contact");
            entity.Property(e => e.UserStatus).HasColumnName("user_status");
            entity.Property(e => e.UserWatsappContact)
                .HasMaxLength(15)
                .HasColumnName("user_watsapp_contact");
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_activity_log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionDescription)
                .HasMaxLength(500)
                .HasColumnName("action_description");
            entity.Property(e => e.ActionType).HasColumnName("action_type");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.OpId).HasColumnName("op_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<UserActivityType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user_activity_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TypeValue)
                .HasMaxLength(100)
                .HasColumnName("type_value");
        });

        modelBuilder.Entity<UsersOtp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users_otp");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.UserId).HasColumnName("user_Id");
            entity.Property(e => e.UserOtp)
                .HasMaxLength(6)
                .HasColumnName("user_OTP");
            entity.Property(e => e.UserOtpStatus).HasColumnName("user_OTP_status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
