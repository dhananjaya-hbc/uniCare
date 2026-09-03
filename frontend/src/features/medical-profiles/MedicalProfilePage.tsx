import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useParams } from 'react-router-dom'
import { toast } from 'sonner'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
    Card, CardContent, CardDescription, CardHeader, CardTitle,
} from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { getApiErrorMessage } from '@/lib/api-client'
import {
    useMedicalProfile, useRejectMedicalProfile,
    useSubmitMedicalProfile, useUpsertMedicalProfile, useVerifyMedicalProfile,
} from './hooks'
import {
    BLOOD_GROUPS, BLOOD_GROUP_LABELS, EDITABLE_STATUSES, STATUS_LABELS,
    type BloodGroup, type UpsertMedicalProfileRequest, type VerificationStatus,
} from './types'

// TODO(auth): the reviewing staff id must come from the signed-in user.
const PLACEHOLDER_STAFF_ID = '7f6b9f58-4764-4a4e-9b00-db373c9f30b0'

function statusVariant(status: VerificationStatus) {
    if (status === 'Verified') return 'default' as const
    if (status === 'Rejected') return 'destructive' as const
    return 'secondary' as const
}

export function MedicalProfilePage() {
    const { studentId = '' } = useParams()

    const { data: profile, isPending, error } = useMedicalProfile(studentId)
    const upsert = useUpsertMedicalProfile(studentId)
    const submit = useSubmitMedicalProfile(studentId)
    const verify = useVerifyMedicalProfile(studentId)
    const reject = useRejectMedicalProfile(studentId)

    const { register, handleSubmit, reset } = useForm<UpsertMedicalProfileRequest>({
        defaultValues: { bloodGroup: 'Unknown' },
    })

    // Populate the form once the profile arrives, and again after every mutation
    // returns a new one — reset() is how react-hook-form adopts server state.
    useEffect(() => {
        if (profile) reset(profile)
    }, [profile, reset])

    // A profile that has been submitted or verified is read-only. This mirrors
    // MedicalProfileService.EditableStates — the server rejects edits anyway;
    // this only stops the user wasting a round trip discovering that.
    const status = profile?.status ?? 'Draft'
    const isEditable = !profile || EDITABLE_STATUSES.includes(status)
    const awaitingReview = status === 'SubmittedForVerification'
    const busy = upsert.isPending || submit.isPending || verify.isPending || reject.isPending

    function onSave(values: UpsertMedicalProfileRequest) {
        upsert.mutate(
            {
                ...values,
                heightCm: values.heightCm ? Number(values.heightCm) : null,
                weightKg: values.weightKg ? Number(values.weightKg) : null,
            },
            {
                onSuccess: () => toast.success('Medical profile saved'),
                onError: (e) => toast.error(getApiErrorMessage(e)),
            },
        )
    }

    function onSubmitForReview() {
        submit.mutate(undefined as never, {
            onSuccess: () => toast.success('Submitted for verification'),
            onError: (e) => toast.error(getApiErrorMessage(e)),
        })
    }

    function onVerify() {
        verify.mutate(PLACEHOLDER_STAFF_ID, {
            onSuccess: () => toast.success('Profile verified'),
            onError: (e) => toast.error(getApiErrorMessage(e)),
        })
    }

    function onReject() {
        const reason = window.prompt('What needs to change?')
        if (!reason?.trim()) return
        reject.mutate(
            { staffId: PLACEHOLDER_STAFF_ID, reason },
            {
                onSuccess: () => toast.success('Changes requested'),
                onError: (e) => toast.error(getApiErrorMessage(e)),
            },
        )
    }

    if (isPending) {
        return <p className="p-6 text-sm text-muted-foreground">Loading medical profile…</p>
    }

    return (
        <div className="mx-auto max-w-3xl p-6">
            <Link to="/students" className="text-xs text-muted-foreground hover:underline">
                ← Back to students
            </Link>

            <div className="mt-3 mb-6 flex flex-wrap items-center justify-between gap-3">
                <div>
                    <h1 className="text-2xl font-semibold">Medical profile</h1>
                    <p className="text-sm text-muted-foreground">
                        {profile?.submittedAt
                            ? `Submitted ${new Date(profile.submittedAt).toLocaleDateString()}`
                            : profile
                                ? 'Draft — not yet submitted'
                                : 'Not started yet'}

                    </p>
                </div>
                <Badge variant={statusVariant(status)}>{STATUS_LABELS[status]}</Badge>
            </div>

            {error && (
                <p className="mb-4 rounded-md bg-destructive/10 p-3 text-sm text-destructive">
                    {getApiErrorMessage(error)}
                </p>
            )}

            {status === 'Rejected' && profile?.rejectionReason && (
                <div className="mb-4 rounded-md border border-destructive bg-destructive/10 p-4">
                    <p className="text-sm font-medium text-destructive">Changes requested</p>
                    <p className="mt-1 text-sm text-foreground">{profile.rejectionReason}</p>
                </div>
            )}

            <form onSubmit={handleSubmit(onSave)}>
                <fieldset disabled={!isEditable || busy} className="grid gap-6 disabled:opacity-70">
                    <Card>
                        <CardHeader>
                            <CardTitle className="text-base">Basic measurements</CardTitle>
                            <CardDescription>Recorded at registration; a nurse confirms them on your first visit.</CardDescription>
                        </CardHeader>
                        <CardContent className="grid gap-4 sm:grid-cols-3">
                            <div className="grid gap-1.5">
                                <Label htmlFor="bloodGroup">Blood group</Label>
                                <select
                                    id="bloodGroup"
                                    className="h-9 rounded-md border border-input bg-transparent px-3 text-sm disabled:cursor-not-allowed"
                                    {...register('bloodGroup')}
                                >
                                    {BLOOD_GROUPS.map((g: BloodGroup) => (
                                        <option key={g} value={g}>{BLOOD_GROUP_LABELS[g]}</option>
                                    ))}
                                </select>
                            </div>
                            <div className="grid gap-1.5">
                                <Label htmlFor="heightCm">Height (cm)</Label>
                                <Input id="heightCm" type="number" step="0.1" {...register('heightCm')} />
                            </div>
                            <div className="grid gap-1.5">
                                <Label htmlFor="weightKg">Weight (kg)</Label>
                                <Input id="weightKg" type="number" step="0.1" {...register('weightKg')} />
                            </div>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader>
                            <CardTitle className="text-base">Medical history</CardTitle>
                            <CardDescription>Anything a doctor should know before treating you.</CardDescription>
                        </CardHeader>
                        <CardContent className="grid gap-4">
                            <div className="grid gap-1.5">
                                <Label htmlFor="allergies">Allergies</Label>
                                <Textarea id="allergies" rows={2}
                                    placeholder="e.g. Penicillin — causes rash and swelling"
                                    {...register('allergies')} />
                            </div>
                            <div className="grid gap-1.5">
                                <Label htmlFor="chronicConditions">Chronic conditions</Label>
                                <Textarea id="chronicConditions" rows={2} {...register('chronicConditions')} />
                            </div>
                            <div className="grid gap-1.5">
                                <Label htmlFor="currentMedications">Current medications</Label>
                                <Textarea id="currentMedications" rows={2} {...register('currentMedications')} />
                            </div>
                        </CardContent>
                    </Card>

                    <Card>
                        <CardHeader>
                            <CardTitle className="text-base">Examinations</CardTitle>
                        </CardHeader>
                        <CardContent className="grid gap-4 sm:grid-cols-2">
                            <div className="grid gap-1.5">
                                <Label htmlFor="eyeExamination">Eye examination</Label>
                                <Textarea id="eyeExamination" rows={2} {...register('eyeExamination')} />
                            </div>
                            <div className="grid gap-1.5">
                                <Label htmlFor="dentalExamination">Dental examination</Label>
                                <Textarea id="dentalExamination" rows={2} {...register('dentalExamination')} />
                            </div>
                        </CardContent>
                    </Card>
                </fieldset>

                <div className="mt-6 flex flex-wrap items-center gap-2">
                    {isEditable && (
                        <>
                            <Button type="submit" disabled={busy}>
                                {upsert.isPending ? 'Saving…' : 'Save'}
                            </Button>
                            <Button type="button" variant="outline" disabled={busy || !profile}
                                onClick={onSubmitForReview}>
                                Submit for verification
                            </Button>
                        </>
                    )}

                    {/* TODO(auth): staff-only — hide behind hasRole once JWT lands. */}
                    {awaitingReview && (
                        <>
                            <Button type="button" disabled={busy} onClick={onVerify}>Verify</Button>
                            <Button type="button" variant="outline" disabled={busy} onClick={onReject}>
                                Request changes
                            </Button>
                        </>
                    )}

                    {status === 'Verified' && (
                        <p className="text-sm text-muted-foreground">
                            Verified by medical staff — contact the centre to request a change.
                        </p>
                    )}
                </div>
            </form>
        </div>
    )
}
