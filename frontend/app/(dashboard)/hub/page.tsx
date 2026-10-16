import type { Metadata } from "next";
import { departmentService } from "@/features/departments/services/DepartmentService";
import HubBoard from "@/features/hub/components/HubBoard";
import { roleService } from "@/features/roles/services/RoleService";
import { userService } from "@/features/users/services/UserService";
import { all } from "@/lib/result";

export const metadata: Metadata = {
  title: "Módulos",
  description: "Todos os módulos do MeOne e as funções de cada um.",
};

export default async function HubPage() {
  const [users, roles, departments] = await Promise.all([
    userService.list({ pageSize: 1 }),
    roleService.list(),
    departmentService.all(),
  ]);

  const data = all({ users, roles, departments });

  const counts = data.ok
    ? {
        "/users": data.value.users.totalCount,
        "/roles": data.value.roles.totalCount,
        "/departments": data.value.departments.length,
      }
    : null;

  return <HubBoard counts={counts} error={data.ok ? null : data.error} />;
}
