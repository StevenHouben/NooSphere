/****************************************************************************
 (c) 2013 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using ABC.Model;

namespace ABC.Infrastructure
{
    public class ActivityEventArgs
    {
        public IActivity Activity { get; set; }
        public ActivityEventArgs() {}

        public ActivityEventArgs( IActivity activity )
        {
            Activity = activity;
        }
    }

    public class ActivityRemovedEventArgs
    {
        public string Id { get; set; }
        public ActivityRemovedEventArgs() {}

        public ActivityRemovedEventArgs( string id )
        {
            Id = id;
        }
    }
}