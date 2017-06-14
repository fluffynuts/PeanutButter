using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class User : EntityBase, IUser
    {
        public UserRole UserRole { get; set; }
        public string FirstNames { get; set; }
        public string Surname { get; set; }
        public string HashedPassword { get; set; }
        public string ContactId { get; set; }
        public string UserName { get; set; }

        public void Returns(Task<User> userTask)
        {
            throw new NotImplementedException();
        }
    }
}