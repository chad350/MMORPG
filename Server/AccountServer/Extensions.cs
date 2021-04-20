using System;
using AccountServer.DB;
using SharedDB;

namespace AccountServer
{
    public static class Extensions
    {
        public static bool SaveChangesEx(this AppDbContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
        public static bool SaveChangesEx(this SharedDbContext db)
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}