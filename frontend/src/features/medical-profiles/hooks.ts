import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  getMedicalProfile, rejectMedicalProfile, submitMedicalProfile,
  upsertMedicalProfile, verifyMedicalProfile,
} from './api'
import type { UpsertMedicalProfileRequest } from './types'

export const medicalProfileKeys = {
  all: ['medical-profiles'] as const,
  byStudent: (studentId: string) => [...medicalProfileKeys.all, studentId] as const,
}

export function useMedicalProfile(studentId: string) {
  return useQuery({
    queryKey: medicalProfileKeys.byStudent(studentId),
    queryFn: () => getMedicalProfile(studentId),
    enabled: Boolean(studentId),
  })
}

/**
 * Every mutation here returns the updated profile, so rather than invalidating
 * and refetching we write the response straight into the cache. One round trip
 * instead of two, and the status badge updates the instant the call returns.
 */
function useProfileMutation<TArgs>(
  studentId: string,
  fn: (args: TArgs) => Promise<Awaited<ReturnType<typeof getMedicalProfile>>>,
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: fn,
    onSuccess: (profile) => {
      queryClient.setQueryData(medicalProfileKeys.byStudent(studentId), profile)
    },
  })
}

export function useUpsertMedicalProfile(studentId: string) {
  return useProfileMutation(studentId, (request: UpsertMedicalProfileRequest) =>
    upsertMedicalProfile(studentId, request))
}

export function useSubmitMedicalProfile(studentId: string) {
  return useProfileMutation(studentId, () => submitMedicalProfile(studentId))
}

export function useVerifyMedicalProfile(studentId: string) {
  return useProfileMutation(studentId, (staffId: string) =>
    verifyMedicalProfile(studentId, staffId))
}

export function useRejectMedicalProfile(studentId: string) {
  return useProfileMutation(studentId, ({ staffId, reason }: { staffId: string; reason: string }) =>
    rejectMedicalProfile(studentId, staffId, reason))
}
