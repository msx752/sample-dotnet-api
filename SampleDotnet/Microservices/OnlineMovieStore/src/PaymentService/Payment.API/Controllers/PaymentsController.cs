using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payment.Database;
using Payment.Database.Entities;
using SampleProject.Contract;
using SampleProject.Contract.Payment;
using SampleProject.Contract.Payment.Cart;
using SampleProject.Core.Model.Base;
using SampleProject.Payment.API.Models.Dtos;
using SampleDotnet.Result;
using SampleDotnet.RepositoryFactory.Interfaces;

namespace SampleProject.ayment.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : BaseController
    {
        private readonly IMessageBus _messageBus;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentsController(
            IMapper _mapper
            , IMessageBus messageBus
            , IUnitOfWork unitOfWork)
            : base(_mapper)
        {
            this._messageBus = messageBus;
            this._unitOfWork = unitOfWork;
        }

        [HttpGet("History")]
        public async Task<IActionResult> PaymentHistory()
        {
            using (var repository = _unitOfWork.CreateRepository<PaymentDbContext>())
            {
                var transactionEntities = await repository
                       .Where<TransactionEntity>(f => f.UserId == LoggedUserId)
                       .Include(f => f.TransactionItems)
                       .ToListAsync();

                return new OkResponse(mapper.Map<List<TransactionDto>>(transactionEntities));
            }
        }

        [HttpPost("Create/{cartId}")]
        public async Task<IActionResult> CreatePayment(Guid cartId)
        {
            var lock_response = await _messageBus.Call<CartStatusResponseMessage, CartStatusRequestMessage>(new()
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
                using (var repository = _unitOfWork.CreateRepository<PaymentDbContext>())
                {
                    var cartEntityResponse = await _messageBus.Call<CartEntityResponseMessage, CartEntityRequestMessage>(new()
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

                    await repository.InsertAsync(transactionEntity);

                    await _unitOfWork.SaveChangesAsync();

                    var paid_response = await _messageBus.Call<CartStatusResponseMessage, CartStatusRequestMessage>(new()
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
}