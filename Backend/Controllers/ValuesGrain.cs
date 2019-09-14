using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Backend.Controllers
{
    public class ValuesGrain : Grain, IValues
    {
        private int value = 0;

        public Task<int> Increment()
        {
            value++;
            return Task.FromResult(value);
        }
    }
}