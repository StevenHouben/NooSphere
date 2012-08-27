/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.Collections.Generic;
using NooSphere.Core.Primitives;

namespace NooSphere.Core.ActivityModel
{
    /// <summary>
    /// Activity Base Class
    /// </summary>
    public class Activity : Noo
    {
        #region Constructors

        public Activity()
        {
            InitializeProperties();
        }

        #endregion

        #region Initializers

        private void InitializeProperties()
        {
            Actions = new List<Action>();
            Participants = new List<User>();
            Meta = new Metadata();
            Resources =  new List<Resource>();
        }

        #endregion

        #region Properties
        public User Owner { get; set; }
        public List<User> Participants { get; set; }
        public List<Action> Actions{ get; set; }
        public Metadata Meta{ get; set; }
        public List<Resource> Resources { get; set; }

        #endregion

        #region Public Methods

        public List<Resource> GetResources()
        {
            return Resources;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Activity act)
        {
            return Id == act.Id;
        }

        #endregion
    }
}