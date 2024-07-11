using AtmApplicationApp.Interface;
using AtmApplicationApp.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace AtmApplicationApp.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AtmController : ControllerBase
    {
        private readonly IAtmService _atmService;
        public AtmController(IAtmService atmService) {
            _atmService = atmService;
        
        }
        [HttpPost("Login")]
        [ProducesResponseType(typeof(SuccessVerifyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SuccessVerifyDTO>> Login(VerifyUserDTO verify)
        {
            try
            {            

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _atmService.AuthService(verify);
                return Ok(result);
            }
            catch (Exception ex)
            {
              
                return BadRequest(new ErrorModel(400, ex.Message));
            }
        }


        [HttpPost("DepositMoney")]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResponseDepositDTO>> DepositMoney([Required] long amount)
        {
            try
            {
                var accountNumber = User.Claims?.FirstOrDefault(x => x.Type == "AccountNumber")?.Value;
                long userid = long.Parse(accountNumber);
                var result = await  _atmService.DepositMoney(userid, amount);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return Unauthorized(new ErrorModel(401, ex.Message));
            }
        }


        [HttpPost("WithdrawMoney")]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<WithdrawDTO>> WithdrawalMoney([Required] double amount)
        {
            try
            {
                var accountNumber = User.Claims?.FirstOrDefault(x => x.Type == "AccountNumber")?.Value;
                long userid = long.Parse(accountNumber);
                var result = await _atmService.WithdrawalMoney(userid, amount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new ErrorModel(401, ex.Message));
            }
        }

        [HttpPost("CheckBalance")]
        [ProducesResponseType(typeof(BalanceDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BalanceDTO>> CheckBalance()
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var accountNumber = User.Claims?.FirstOrDefault(x => x.Type == "AccountNumber")?.Value;
                long userid = long.Parse(accountNumber);

                var result = await _atmService.CheckBalance(userid);
                return Ok(result);
            }
            catch (Exception ex)
            {

                return Unauthorized(new ErrorModel(401, ex.Message));
            }
        }
        [HttpGet("GetTransaction")]
        [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<TransactionDto>>> GetTransaction()
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var accountNumber = User.Claims?.FirstOrDefault(x => x.Type == "AccountNumber")?.Value;
                long userid = long.Parse(accountNumber);

                var result = await _atmService.GetTransaction(userid);
                return Ok(result);
            }
            catch (Exception ex)
            {

                return Unauthorized(new ErrorModel(401, ex.Message));
            }
        }






    }
}
