Public Enum ContentManagerType
    NotSet = 0
    Image = 1
    Audio = 2
    Video = 3
    GeneralFile = 4
End Enum

Public Enum ManagerViewMode
    allVersions = 1
    defaultVersions = 2
End Enum

Public Enum Enums
    success = 0
    fail = 1
    missingParameters = 2
    parameterInvalid = 3
    conflict = 4
    notFound = 5
End Enum

Public Enum GamePeriodStatus
    Queued = 0
    Progress = 1
    Ended = 2
End Enum

Public Enum ProgressStatusEnum
    notStarted = 0
    inProgress = 1
    completed = 2
End Enum

Public Enum CashflowType
    revenue = 0
    cost = 1
End Enum

Public Enum PaymentType
    oneTime = 0
    weeklyRecursive = 1
    monthlyRecursive = 2
    yearlyRecursive = 3
End Enum

Public Enum PaymentTargetType
    asset = 0
    liability = 1
    equity = 2
End Enum

Public Enum BusinessAspect
    marketing = 0
    rnd = 1
    production = 2
End Enum

Public Enum IdType
    revenueAndCost = 1
End Enum