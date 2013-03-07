using System.Collections.Generic;

using System.Diagnostics.Contracts;



namespace Sando.ExtensionContracts.QueryContracts
{

    public partial interface IQueryWeightsSupplier
    {
        Dictionary<string, float> GetQueryWeightsValues();
    }

    #region IQueryWeightsSupplier contract binding
    [ContractClass(typeof(IQueryWeightsSupplierContract))]
    public partial interface IQueryWeightsSupplier
    {

    }

    [ContractClassFor(typeof(IQueryWeightsSupplier))]
    abstract class IQueryWeightsSupplierContract : IQueryWeightsSupplier
    {

        public Dictionary<string, float> GetQueryWeightsValues()
        {
            Contract.Ensures(Contract.Result<Dictionary<string, float>>() != null, "IQueryWeightsSupplier:GetQueryWeightsValues - an object must be returned from this method!");
            throw new System.NotImplementedException();
        }

    }
    #endregion

}

