﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DataFramework.DTO;
using ControllerVulkano;


namespace VulkanoPruebasAutomatizadas_Back.Controllers
{

    public class MQTipoPruebaController : ApiController
    {
        // GET: api/MQTipoPrueba
        [HttpGet]
        public IEnumerable<MQTipoPruebaDTO> Get()
        {
            ControllerVulkano.MQTipoPruebaController mQTipoPruebaController = new ControllerVulkano.MQTipoPruebaController();
            return mQTipoPruebaController.SelectMQTipoPrueba(0);
        }

        // GET: api/MQTipoPrueba/5
        //[HttpGet("{id}", Name = "GetMQTipoPrueba")]
        public MQTipoPruebaDTO GetMQTipoPrueba(int id)
        {
            ControllerVulkano.MQTipoPruebaController mQTipoPruebaController = new ControllerVulkano.MQTipoPruebaController();
            return mQTipoPruebaController.SelectMQTipoPrueba(id).First();
        }

        // POST: api/MQTipoPrueba
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/MQTipoPrueba/5
        //[HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        //[HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
