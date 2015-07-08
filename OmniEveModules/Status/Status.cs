using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniEveModules.Status
{
    using DirectEve;
    using OmniEveModules.Caching;
    using OmniEveModules.Logging;
    using OmniEveModules.Lookup;

    public class Status
    {
        /// <summary>
        ///   Singleton implementation
        /// </summary>
        private static readonly Status _instance = new Status();

        public static Status Instance
        {
            get { return _instance; }
        }

        public bool InSpace
        {
            get
            {
                try
                {
                    if (DateTime.UtcNow < Time.Instance.LastSessionChange.AddSeconds(10))
                    {
                        return false;
                    }

                    if (DateTime.UtcNow < Time.Instance.LastInSpace.AddMilliseconds(800))
                    {
                        //if We already set the LastInStation timestamp this iteration we do not need to check if we are in station
                        return true;
                    }

                    if (Cache.Instance.DirectEve.Session.IsInSpace && !Cache.Instance.DirectEve.Session.IsInStation && Cache.Instance.DirectEve.Session.IsReady)
                    {
                        Time.Instance.LastInSpace = DateTime.UtcNow;
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Logging.Log("Status:InSpace", "if (DirectEve.Session.IsInSpace && !DirectEve.Session.IsInStation && DirectEve.Session.IsReady) <---must have failed exception was [" + ex.Message + "]", Logging.Teal);
                    return false;
                }
            }
        }

        public bool InStation
        {
            get
            {
                try
                {
                    if (DateTime.UtcNow < Time.Instance.LastSessionChange.AddSeconds(10))
                    {
                        return false;
                    }

                    if (DateTime.UtcNow < Time.Instance.LastInStation.AddMilliseconds(800))
                    {
                        //if We already set the LastInStation timestamp this iteration we do not need to check if we are in station
                        return true;
                    }

                    if (Cache.Instance.DirectEve.Session.IsInStation && !Cache.Instance.DirectEve.Session.IsInSpace && Cache.Instance.DirectEve.Session.IsReady)
                    {
                        Time.Instance.LastInStation = DateTime.UtcNow;
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Logging.Log("Status:InStation", "if (DirectEve.Session.IsInStation && !DirectEve.Session.IsInSpace && DirectEve.Session.IsReady) <---must have failed exception was [" + ex.Message + "]", Logging.Teal);
                    return false;
                }
            }
        }

        public bool InWarp
        {
            get
            {
                try
                {
                    if (InSpace && !InStation)
                    {
                        if (Cache.Instance.DirectEve.ActiveShip != null)
                        {
                            if (Cache.Instance.DirectEve.ActiveShip.Entity != null)
                            {
                                if (Cache.Instance.DirectEve.ActiveShip.Entity.Mode == 3)
                                {
                                    Time.Instance.LastInWarp = DateTime.UtcNow;
                                    return true;
                                }
                                else
                                {
                                    Logging.Log("Status:InWarp", "We are not in warp.Cache.Instance.ActiveShip.Entity.Mode  is [" + Cache.Instance.DirectEve.ActiveShip.Entity.Mode + "]", Logging.Teal);
                                    return false;
                                }
                            }
                            else
                            {
                                Logging.Log("Status:InWarp", "Why are we checking for InWarp if Cache.Instance.ActiveShip.Entity is Null? (session change?)", Logging.Teal);
                                return false;
                            }
                        }
                        else
                        {
                            Logging.Log("Status:InWarp", "Why are we checking for InWarp if Cache.Instance.ActiveShip is Null? (session change?)", Logging.Teal);
                            return false;
                        }
                    }
                    else
                    {
                        Logging.Log("Status:InWarp", "Why are we checking for InWarp while docked or between session changes?", Logging.Teal);
                        return false;
                    }
                }
                catch (Exception exception)
                {
                    Logging.Log("Status:InWarp", "InWarp check failed, exception [" + exception + "]", Logging.Teal);
                }

                return false;
            }
        }

        /*public bool IsOrbiting(long EntityWeWantToBeOrbiting = 0)
        {
            try
            {
                if (Cache.Instance.Approaching != null)
                {
                    bool _followIDIsOnGrid = false;

                    if (EntityWeWantToBeOrbiting != 0)
                    {
                        _followIDIsOnGrid = (EntityWeWantToBeOrbiting == Cache.Instance.ActiveShip.Entity.FollowId);
                    }
                    else
                    {
                        _followIDIsOnGrid = Cache.Instance.EntitiesOnGrid.Any(i => i.Id == Cache.Instance.ActiveShip.Entity.FollowId);
                    }

                    if (Cache.Instance.ActiveShip.Entity != null && Cache.Instance.ActiveShip.Entity.Mode == 4 && _followIDIsOnGrid)
                    {
                        return true;
                    }

                    return false;
                }

                return false;
            }
            catch (Exception exception)
            {
                Logging.Log("Status:IsApproaching", "Exception [" + exception + "]", Logging.Debug);
                return false;
            }
        }*/

        /*public bool IsApproaching(long EntityWeWantToBeApproaching = 0)
        {
            try
            {
                if (Cache.Instance.Approaching != null)
                {
                    bool _followIDIsOnGrid = false;

                    if (EntityWeWantToBeApproaching != 0)
                    {
                        _followIDIsOnGrid = (EntityWeWantToBeApproaching == Cache.Instance.ActiveShip.Entity.FollowId);
                    }
                    else
                    {
                        _followIDIsOnGrid = Cache.Instance.EntitiesOnGrid.Any(i => i.Id == Cache.Instance.ActiveShip.Entity.FollowId);
                    }

                    if (Cache.Instance.ActiveShip.Entity != null && Cache.Instance.ActiveShip.Entity.Mode == 1 && _followIDIsOnGrid)
                    {
                        return true;
                    }

                    return false;
                }

                return false;
            }
            catch (Exception exception)
            {
                Logging.Log("Status:IsApproaching", "Exception [" + exception + "]", Logging.Debug);
                return false;
            }
        }*/

        /*public bool IsApproachingOrOrbiting(long EntityWeWantToBeApproachingOrOrbiting = 0)
        {
            try
            {
                if (IsApproaching(EntityWeWantToBeApproachingOrOrbiting))
                {
                    return true;
                }

                if (IsOrbiting(EntityWeWantToBeApproachingOrOrbiting))
                {
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Logging.Log("Status:IsApproachingOrOrbiting", "Exception [" + exception + "]", Logging.Debug);
                return false;
            }
        }*/
    }
}
