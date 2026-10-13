using System;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MeOne.Infrastructure.Migrations
{
    [DbContext(typeof(MeOneDbContext))]
    [Migration("20261013131123_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("MeOne.Domain.Entities.ClientApplication", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("active");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("applications", (string)null);
                });

            modelBuilder.Entity("MeOne.Domain.Entities.Department", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("departments", (string)null);
                });

            modelBuilder.Entity("MeOne.Domain.Entities.RefreshToken", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("expires");

                    b.Property<bool>("Revoked")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("revoked");

                    b.Property<string>("TokenHash")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("token");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("fk_user");

                    b.HasKey("Id");

                    b.HasIndex("TokenHash")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("refresh_tokens", (string)null);
                });

            modelBuilder.Entity("MeOne.Domain.Entities.Role", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("roles", (string)null);

                    b.HasData(
                        new
                        {
                            Id = 1ul,
                            Description = "Administração",
                            Name = "master"
                        });
                });

            modelBuilder.Entity("MeOne.Domain.Entities.User", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("active");

                    b.Property<int>("DepartmentId")
                        .HasColumnType("int")
                        .HasColumnName("fk_department");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)")
                        .HasColumnName("email");

                    b.Property<DateTime>("ModifiedAt")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("modified_at");

                    b.Property<ulong>("ModifiedBy")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("modified_by");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("name");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)")
                        .HasColumnName("password");

                    b.Property<uint>("Registration")
                        .HasColumnType("int unsigned")
                        .HasColumnName("registration");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Registration")
                        .IsUnique();

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("MeOne.Domain.Entities.UserApplication", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<ulong>("ApplicationId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("fk_application");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("fk_user");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId");

                    b.HasIndex("UserId");

                    b.ToTable("user_applications", (string)null);
                });

            modelBuilder.Entity("MeOne.Domain.Entities.UserRole", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<ulong>("RoleId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("fk_role");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned")
                        .HasColumnName("fk_user");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId", "RoleId")
                        .IsUnique();

                    b.ToTable("user_roles", (string)null);
                });

            modelBuilder.Entity("MeOne.Domain.Entities.RefreshToken", b =>
                {
                    b.HasOne("MeOne.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("refresh_tokens_fk_user");
                });

            modelBuilder.Entity("MeOne.Domain.Entities.User", b =>
                {
                    b.HasOne("MeOne.Domain.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("users_fk_department");

                    b.Navigation("Department");
                });

            modelBuilder.Entity("MeOne.Domain.Entities.UserApplication", b =>
                {
                    b.HasOne("MeOne.Domain.Entities.ClientApplication", "Application")
                        .WithMany()
                        .HasForeignKey("ApplicationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("user_applications_fk_application");

                    b.HasOne("MeOne.Domain.Entities.User", null)
                        .WithMany("Applications")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("user_applications_fk_user");

                    b.Navigation("Application");
                });

            modelBuilder.Entity("MeOne.Domain.Entities.UserRole", b =>
                {
                    b.HasOne("MeOne.Domain.Entities.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("user_roles_fk_role");

                    b.HasOne("MeOne.Domain.Entities.User", null)
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("user_roles_fk_user");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("MeOne.Domain.Entities.User", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("Roles");
                });
#pragma warning restore 612, 618
        }
    }
}
