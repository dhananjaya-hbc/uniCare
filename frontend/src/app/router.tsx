import { Navigate, Route, Routes } from 'react-router-dom'
import { ROLES, STAFF_ROLES } from '@/config/roles'
import { ROUTES } from '@/config/routes'
import { MedicalProfilePage } from '@/features/medical-profiles/MedicalProfilePage'
import { StudentsPage } from '@/features/students/StudentsPage'
import { SystemStatusPage } from '@/features/system/SystemStatusPage'
import { AuthLayout } from '@/layouts/AuthLayout'
import { StaffLayout } from '@/layouts/StaffLayout'
import { StudentLayout } from '@/layouts/StudentLayout'
import { ProtectedRoute } from './ProtectedRoute'

/**
 * The whole route tree. Guards wrap route *groups*, so adding a page inside a
 * group inherits its protection automatically — you cannot forget to guard it.
 */
export function AppRouter() {
  return (
    <Routes>
      {/* TODO(auth): wrap this group in <ProtectedRoute allowedRoles={STAFF_ROLES} />
          once login exists. Until then these pages are reachable by anyone. */}
      <Route element={<StaffLayout />}>
        <Route path={ROUTES.systemStatus} element={<SystemStatusPage />} />
        <Route path="/students" element={<StudentsPage />} />
        <Route path="/students/:studentId/medical-profile" element={<MedicalProfilePage />} />
      </Route>

      <Route element={<AuthLayout />}>
        {/* TODO(auth): <Route path={ROUTES.login} element={<LoginPage />} /> */}
      </Route>

      {/* Student area — empty until login exists; ProtectedRoute redirects everything. */}
      <Route element={<ProtectedRoute allowedRoles={[ROLES.Student]} />}>
        <Route element={<StudentLayout />}>
          {/* TODO: student dashboard, medical profile, documents, appointments */}
        </Route>
      </Route>

      {/* Staff area — populated once the group above moves behind the guard. */}
      <Route element={<ProtectedRoute allowedRoles={STAFF_ROLES} />}>
        <Route element={<StaffLayout />}>
          {/* TODO: staff dashboard, appointments, queue, pharmacy, lab */}
        </Route>
      </Route>

      {/* Until login exists, land on the status page so the app is verifiable. */}
      <Route path="*" element={<Navigate to={ROUTES.systemStatus} replace />} />
    </Routes>
  )
}
