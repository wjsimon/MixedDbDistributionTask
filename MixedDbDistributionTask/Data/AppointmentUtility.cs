using System.Data.Common;

namespace MixedDbDistributionTask.Data
{
    public static class AppointmentUtility
    {
        public static AppointmentDto DTO(DbDataReader dbReader, TherapistDto therapist, PatientDto patient, PracticeDto practice, RemedyDto? remedy = null)
        {
            return new AppointmentDto()
            {
                Id = dbReader.GetString(0),
                StartTime = dbReader.GetString(1),
                EndTime = dbReader.GetString(2),
                Therapist = therapist,
                Patient = patient,
                Practice = practice,
                Remedy = remedy
            };
        }

        public readonly record struct DbStub(
            string Id, string StartTime, string EndTime, string TherapistId, string PatientKv, string PracticeIk, string RemedyDiagnosis);
    }
}
