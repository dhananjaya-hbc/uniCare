import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {Table, TableBody, TableCell, TableHead, TableHeader, TableRow,} from '@/components/ui/table'
import { getApiErrorMessage } from '@/lib/api-client'
import { CreateStudentDialog } from './components/CreateStudentDialog'
import { useStudents } from './hooks'

const PAGE_SIZE = 10

export function StudentsPage() {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)

  const { data, error, isPending, isFetching } = useStudents({
    search: search || undefined,
    page,
    pageSize: PAGE_SIZE,
  })

  function onSearchChange(value: string) {
    setSearch(value)
    setPage(1)   // a new search must start at page 1, or you land on an empty page
  }

  return (
    <div className="mx-auto max-w-5xl p-6">
      <div className="mb-6 flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold">Students</h1>
          <p className="text-sm text-muted-foreground">
            {data ? `${data.totalCount} registered` : 'Loading…'}
          </p>
        </div>
        <CreateStudentDialog />
      </div>

      <div className="mb-4 flex items-center gap-3">
        <Input
          placeholder="Search by name or registration number…"
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          className="max-w-sm"
        />
        {isFetching && !isPending && (
          <span className="text-xs text-muted-foreground">Updating…</span>
        )}
      </div>

      {error && (
        <p className="mb-4 rounded-md bg-destructive/10 p-3 text-sm text-destructive">
          {getApiErrorMessage(error)}
        </p>
      )}

      <div className="overflow-x-auto rounded-md border border-border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Registration</TableHead>
              <TableHead>Name</TableHead>
              <TableHead>Faculty</TableHead>
              <TableHead>Department</TableHead>
              <TableHead className="text-right">Year</TableHead>
              <TableHead>Gender</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isPending && (
              <TableRow>
                <TableCell colSpan={6} className="text-center text-muted-foreground">
                  Loading students…
                </TableCell>
              </TableRow>
            )}

            {data?.items.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="text-center text-muted-foreground">
                  {search ? `No students match “${search}”.` : 'No students registered yet.'}
                </TableCell>
              </TableRow>
            )}

            {data?.items.map((student) => (
              <TableRow key={student.id}>
                <TableCell className="font-mono text-xs">{student.registrationNumber}</TableCell>
                <TableCell className="font-medium">
                  <Link
                    to={`/students/${student.id}/medical-profile`}
                    className="hover:underline"
                  >
                    {student.fullName}
                  </Link>
                </TableCell>
                <TableCell>{student.faculty}</TableCell>
                <TableCell>{student.department}</TableCell>
                <TableCell className="text-right tabular-nums">{student.academicYear}</TableCell>
                <TableCell><Badge variant="secondary">{student.gender}</Badge></TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {data && data.totalPages > 1 && (
        <div className="mt-4 flex items-center justify-between">
          <p className="text-xs text-muted-foreground">
            Page {data.page} of {data.totalPages}
          </p>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" disabled={page <= 1}
              onClick={() => setPage((p) => p - 1)}>
              Previous
            </Button>
            <Button variant="outline" size="sm" disabled={page >= data.totalPages}
              onClick={() => setPage((p) => p + 1)}>
              Next
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
