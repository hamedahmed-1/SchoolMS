using AutoMapper;
using SchoolMS.DTO;
using SchoolMS.Models;

namespace SchoolMS.Mappings
{
    public class StudentProfile : Profile
    {
        //public StudentProfile()
        //{
        //    CreateMap<Student, StudentDTO>().ReverseMap();
        //    CreateMap<Grade, GradeDto>().ReverseMap();
        //    CreateMap<Fee, FeeDto>().ReverseMap();
        //    CreateMap<Installment, InstallmentDTO>().ReverseMap();

        //    CreateMap<StudentDTO, Student>()
        //    .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ensure Id is not mapped

        //    CreateMap<Fee, FeeDto>()
        //    .ForMember(dest => dest.AmountPerInstallment, opt => opt.MapFrom(src => src.TotalAmount / src.NumberOfInstallments));

        //}

        public StudentProfile()
        {
            //// Map between Student and StudentDTO
            //CreateMap<Student, StudentDTO>()
            //    .ForMember(dest => dest.GradeId, opt => opt.MapFrom(src => src.GradeId))
            //    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            //    .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth));

            //// Map between StudentDTO and Student
            //CreateMap<StudentDTO, Student>()
            //    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id is usually auto-generated
            //    .ForMember(dest => dest.Grade, opt => opt.Ignore()) // Ignoring Grade since it should be set by GradeId
            //    .ForMember(dest => dest.Fees, opt => opt.Ignore()); // Ignoring Fees collection for now

            //// Map between Grade and GradeDto
            //CreateMap<Grade, GradeDto>()
            //    .ForMember(dest => dest.GradeName, opt => opt.MapFrom(src => src.Name));

            //// Map between Fee and FeeDto
            //CreateMap<Fee, FeeDto>()
            //    .ForMember(dest => dest.AmountPerInstallment, opt => opt.MapFrom(src => src.AmountPerInstallment))
            //    .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
            //    .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
            //    .ForMember(dest => dest.NumberOfInstallments, opt => opt.MapFrom(src => src.NumberOfInstallments));

            //// Map between Installment and InstallmentDTO
            //CreateMap<Installment, InstallmentDTO>()
            //    .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
            //    .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid))
            //    .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate))
            //    .ForMember(dest => dest.RemainingBalance, opt => opt.MapFrom(src => src.RemainingBalance))
            //    .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.IsPaid));

            // Map Student to StudentDTO and vice versa
            CreateMap<Student, StudentDTO>()
                .ForMember(dest => dest.GradeId, opt => opt.MapFrom(src => src.Grade.Id));
            CreateMap<StudentDTO, Student>();

            // Map Grade to GradeDto and vice versa
            CreateMap<Grade, GradeDto>()
                .ForMember(dest => dest.GradeName, opt => opt.MapFrom(src => src.Name));
            CreateMap<GradeDto, Grade>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GradeName));

            // Map Fee to FeeDto and vice versa
            CreateMap<Fee, FeeDto>()
                .ForMember(dest => dest.AmountPerInstallment, opt => opt.MapFrom(src => src.AmountPerInstallment));
            CreateMap<FeeDto, Fee>()
                .ForMember(dest => dest.AmountPerInstallment, opt => opt.Ignore());

            // Map Installment to InstallmentDTO and vice versa
            CreateMap<Installment, InstallmentDTO>()
                .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid))
                .ForMember(dest => dest.RemainingBalance, opt => opt.MapFrom(src => src.RemainingBalance))
                .ForMember(dest => dest.FeeId, opt => opt.MapFrom(src => src.FeeId))
                .ForMember(dest => dest.Fee, opt => opt.MapFrom(src => src.Fee))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.IsPaid))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate));

            CreateMap<InstallmentDTO, Installment>()
                .ForMember(dest => dest.AmountPaid, opt => opt.MapFrom(src => src.AmountPaid))
                .ForMember(dest => dest.RemainingBalance, opt => opt.MapFrom(src => src.RemainingBalance))
                .ForMember(dest => dest.FeeId, opt => opt.MapFrom(src => src.FeeId))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => src.IsPaid))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate));

            CreateMap<EducationalStage, EducationalStageDto>();
            CreateMap<EducationalStageDto, EducationalStage>();
            
        }
    }
}
