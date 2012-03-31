using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Euler
{
	public class Problem0010Resolver : IResolver
	{
		public Problem0010Resolver(long max)
		{
			this.max = max;
		}

		public string Resolve()
		{
			var primes = new List<long>();
			primes.Add(2);
			long nextPrime = 3;
			long result = 2;
			while(nextPrime < max)
			{
				bool primeFound = false;
				bool notPrime = false;
				while(!primeFound && nextPrime < max)
				{
					long maxDivisor = (long)Math.Floor(Math.Sqrt(nextPrime));
					for(int j = 0; primes[j] <= maxDivisor; ++j)
					{
						if(nextPrime % primes[j] == 0)
						{
							notPrime = true;
							break;
						}
					}
					if(!notPrime)
					{
						primeFound = true;
						primes.Add(nextPrime);
						result += nextPrime;
						nextPrime += 2;
					}
					else
					{
						nextPrime += 2;
						notPrime = false;
					}
				}
			}
			return result.ToString();
		}

		private long max;
	}
}
