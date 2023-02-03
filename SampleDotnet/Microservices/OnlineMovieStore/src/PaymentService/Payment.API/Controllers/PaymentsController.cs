using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleProject.Contract;
using SampleProject.Contract.Payment;
using SampleProject.Contract.Payment.Cart;
using SampleProject.Core.Interfaces.Repositories;
using SampleProject.Core.Model.Base;
using SampleProject.Result;
using SampleProject.Payment.API.Models.Dtos;
using SampleProject.Payment.Database.Entities;
using SampleProject.Payment.Database.Migrations;
using Microsoft.AspNetCore.Authorization;

namespace SampleProject.ayment.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : BaseController
    {
        private readonly IRepository<PaymentDbContext> _repository;
        private readonly IMessageBus messageBus;

        public PaymentsController(
            IMapper _mapper
            , IRepository<PaymentDbContext> repository
            , IMessageBus messageBus)
            : base(_mapper)
        {
            this._repository = repository;
            this.messageBus = messageBus;
        }

        [HttpGet("History")]
        public IActionResult PaymentHistory()
        {
            var transactionEntities = _repository
                   .Where<TransactionEntity>(f => f.UserId == LoggedUserId)
                   .Include(f => f.TransactionItems)
                   .ToList();

            return new OkResponse(mapper.Map<List<TransactionDto>>(transactionEntities));
        }

        [HttpPost("Create/{cartId}")]
        public async Task<IActionResult> CreatePayment(Guid cartId)
        {
            var lock_response = await messageBus.Call<CartStatusResponseMessage, CartStatusRequestMessage>(new()
            {
                CartStatus = "LockedOnPayment",
                CartId = cartId,
                ActivityUserId = LoggedUserId,
            });

            if (!string.IsNullOrEmpty(lock_response.Message.BusErrorMessage))
            {
                return new BadRequestResponse(lock_response.Message.BusErrorMessage);
            }
            else
            {
                var cartEntityResponse = await messageBus.Call<CartEntityResponseMessage, CartEntityRequestMessage>(new()
                {
                    ActivityUserId = LoggedUserId,
                    CartId = cartId,
                });

                if (!string.IsNullOrEmpty(cartEntityResponse.Message.BusErrorMessage))
                {
                    return new BadRequestResponse(cartEntityResponse.Message.BusErrorMessage);
                }

                TransactionEntity transactionEntity = new TransactionEntity();
                transactionEntity.UserId = LoggedUserId;
                transactionEntity.CartId = cartId;

                double totalPrice = 0;
                foreach (var item in cartEntityResponse.Message.Items.GroupBy(f => f.ProductId))
                {
                    double calculatedPrice = Math.Round((item.Count() * item.First().SalesPrice), 2);
                    TransactionItemEntity transactionItemEntity = new TransactionItemEntity()
                    {
                        Transaction = transactionEntity,
                        ProductTitle = item.First().Title,
                        ProductId = item.First().ProductId,
                        ProductPrice = item.First().SalesPrice,
                        ProductPriceCurrency = item.First().SalesPriceCurrency,
                        Quantity = item.Count(),
                        CalculatedPrice = $"{calculatedPrice} {item.First().SalesPriceCurrency}",
                    };

                    transactionEntity.TransactionItems.Add(transactionItemEntity);

                    totalPrice += calculatedPrice;
                }
                transactionEntity.TotalCalculatedPrice = $"{totalPrice} {transactionEntity.TransactionItems.First().ProductPriceCurrency}";

                _repository.Insert(transactionEntity);
                _repository.SaveChanges();

                var paid_response = await messageBus.Call<CartStatusResponseMessage, CartStatusRequestMessage>(new()
                {
                    CartStatus = "Paid",
                    CartId = cartId,
                    ActivityUserId = LoggedUserId,
                });

                if (!string.IsNullOrEmpty(paid_response.Message.BusErrorMessage))
                {
                    return new BadRequestResponse(paid_response.Message.BusErrorMessage);
                }

                return new OkResponse($"successfully paid '{transactionEntity.TotalCalculatedPrice}'");
            }
        }
    }
}