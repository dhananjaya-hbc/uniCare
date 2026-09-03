/** Mirrors UniCare.Domain.Enums.BloodGroup. */
export type BloodGroup =
  | 'Unknown' | 'APositive' | 'ANegative' | 'BPositive' | 'BNegative'
  | 'ABPositive' | 'ABNegative' | 'OPositive' | 'ONegative'

export const BLOOD_GROUPS: BloodGroup[] = [
  'Unknown', 'APositive', 'ANegative', 'BPositive', 'BNegative',
  'ABPositive', 'ABNegative', 'OPositive', 'ONegative',
]

/** Human-readable labels — the API speaks enum names, people read "A+". */
export const BLOOD_GROUP_LABELS: Record<BloodGroup, string> = {
  Unknown: 'Unknown',
  APositive: 'A+', ANegative: 'A−',
  BPositive: 'B+', BNegative: 'B−',
  ABPositive: 'AB+', ABNegative: 'AB−',
  OPositive: 'O+', ONegative: 'O−',
}

/** Mirrors UniCare.Domain.Enums.VerificationStatus. */
export type VerificationStatus =
  | 'Draft' | 'SubmittedForVerification' | 'Verified' | 'Rejected'

export const STATUS_LABELS: Record<VerificationStatus, string> = {
  Draft: 'Draft',
  SubmittedForVerification: 'Awaiting verification',
  Verified: 'Verified',
  Rejected: 'Changes requested',
}

/** The states in which a student may still edit — mirrors MedicalProfileService. */
export const EDITABLE_STATUSES: VerificationStatus[] = ['Draft', 'Rejected']

export interface MedicalProfile {
  id: string
  studentId: string
  bloodGroup: BloodGroup
  heightCm: number | null
  weightKg: number | null
  chronicConditions: string | null
  allergies: string | null
  currentMedications: string | null
  eyeExamination: string | null
  dentalExamination: string | null
  status: VerificationStatus
  submittedAt: string | null
  verifiedAt: string | null
  rejectionReason: string | null
}

export interface UpsertMedicalProfileRequest {
  bloodGroup: BloodGroup
  heightCm?: number | null
  weightKg?: number | null
  chronicConditions?: string | null
  allergies?: string | null
  currentMedications?: string | null
  eyeExamination?: string | null
  dentalExamination?: string | null
}
