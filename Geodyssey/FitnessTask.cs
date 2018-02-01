using System;
using System.Collections.Generic;
using System.Text;

namespace Geodyssey
{
    /// <summary>
    /// A base class for objects which encapsulate the
    /// task of computing an Individual's Fitness
    /// </summary>
    abstract class FitnessTask
    {
        private Individual individual;

        public FitnessTask(Individual individual)
        {
            this.individual = individual;
        }



        #region Methods

        public abstract void Execute();

        #endregion
    }
}
