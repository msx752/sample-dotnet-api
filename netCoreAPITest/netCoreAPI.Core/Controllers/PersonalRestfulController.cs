﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using netCoreAPI.Core.Controllers.Base;
using netCoreAPI.Model.Dtos;
using netCoreAPI.Model.Entities;
using netCoreAPI.Model.Models;
using netCoreAPI.Model.ResponseModels;
using netCoreAPI.Static.Services;
using System.Collections.Generic;
using System.Linq;

namespace netCoreAPI.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonalRestfulController : MainController
    {
        public PersonalRestfulController(ISharedRepository myRepository, IMapper mapper)
            : base(myRepository, mapper)
        {
        }

        /// <summary>
        ///  DELETE: api/PersonalRestful/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ActionResult<BaseResponseModel<PersonalDto>> Delete(int id)
        {
            var personal = MyRepo.Db<Personal>().GetById(id);
            if (personal == null)
                return new NotFoundResponseModel<PersonalDto>();

            personal = MyRepo.Db<Personal>().Delete(personal).Entity;
            MyRepo.SaveChanges();
            return new SuccessResponseModel<PersonalDto>(Mapper.Map<PersonalDto>(personal));
        }

        /// <summary>
        ///  GET: api/PersonalRestful/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<BaseResponseModel<PersonalDto>> Get(int id)
        {
            var personal = MyRepo.Db<Personal>().GetById(id);
            if (personal == null)
                return new NotFoundResponseModel<PersonalDto>();

            return new SuccessResponseModel<PersonalDto>(Mapper.Map<PersonalDto>(personal));
        }

        /// <summary>
        ///  GET: api/PersonalRestful
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<BaseResponseModel<PersonalDto>> Get()
        {
            return new SuccessResponseModel<PersonalDto>(Mapper.Map<List<PersonalDto>>(MyRepo.Db<Personal>().All().ToList()));
        }

        /// <summary>
        ///  POST: api/PersonalRestful
        /// </summary>
        /// <param name="personalViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<BaseResponseModel<PersonalDto>> Post([FromBody] PersonalModel personalViewModel)
        {
            if (!ModelState.IsValid)
                return new BadRequestResponseModel<PersonalDto>(ModelState.Values.SelectMany(v => v.Errors));

            var personalEntity = Mapper.Map<Personal>(personalViewModel);

            var personal = MyRepo.Db<Personal>().Add(personalEntity);
            MyRepo.SaveChanges();
            /*
             To protect from overposting attacks, please enable the specific properties you want to bind to, for
             more details see https://aka.ms/RazorPagesCRUD.
            */
            return new SuccessResponseModel<PersonalDto>(Mapper.Map<PersonalDto>(personal.Entity));
        }

        /// <summary>
        ///  PUT: api/PersonalRestful/5
        /// </summary>
        /// <param name="id"></param>
        /// <param name="personalViewModel"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public ActionResult<BaseResponseModel<PersonalDto>> Put(int id, [FromBody] PersonalModel personalViewModel)
        {
            if (!ModelState.IsValid)
                return new BadRequestResponseModel<PersonalDto>(ModelState.Values.SelectMany(v => v.Errors));
            var personalEntityDb = MyRepo.Db<Personal>().GetById(id);

            if (personalEntityDb == null)
                return new BadRequestResponseModel<PersonalDto>("entity not found");

            var personalEntity = Mapper.Map<Personal>(personalViewModel);
            personalEntity.Id = id;
            /*
             To protect from overposting attacks, please enable the specific properties you want to bind to, for
             more details see https://aka.ms/RazorPagesCRUD.
             */
            personalEntity = MyRepo.Db<Personal>().Update(personalEntity).Entity;
            MyRepo.SaveChanges();
            return new SuccessResponseModel<PersonalDto>();
        }
    }
}