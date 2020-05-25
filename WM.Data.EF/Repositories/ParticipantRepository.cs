using System;
using System.Collections.Generic;
using System.Text;
using WM.Data.Entities;
using WM.Data.IRepositories;

namespace WM.Data.EF.Repositories
{
    public class ParticipantRepository : EFRepository<Participant, int>, IParticipantRepository
    {
        public ParticipantRepository(AppDbContext context) : base(context)
        {
        }
    }
}
