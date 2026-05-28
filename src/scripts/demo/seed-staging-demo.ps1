[CmdletBinding()]
param(
    [string]$BaseUrl = "https://staging-api.bakimsuite.com",
    [string]$TenantIdentifier = "demo-garajpass",
    [string]$OwnerEmail = "demo-owner@garajpass.test",
    [string]$OwnerPassword = $env:MOTORCARE_DEMO_OWNER_PASSWORD,
    [string]$InvitedUserEmail = "demo-tech@garajpass.test"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:ApiBaseUrl = $BaseUrl.TrimEnd("/")
$script:AccessToken = $null

function Write-Step {
    param([string]$Message)

    Write-Host "[demo-seed] $Message"
}

function Get-StatusCode {
    param($ErrorRecord)

    if ($null -ne $ErrorRecord.Exception.Response -and
        $null -ne $ErrorRecord.Exception.Response.StatusCode) {
        return [int]$ErrorRecord.Exception.Response.StatusCode
    }

    return $null
}

function Invoke-MotorCareApi {
    param(
        [Parameter(Mandatory)]
        [ValidateSet("Get", "Post", "Put", "Patch")]
        [string]$Method,

        [Parameter(Mandatory)]
        [string]$Path,

        [object]$Body = $null,

        [switch]$AllowNotFound
    )

    $headers = @{}
    if (-not [string]::IsNullOrWhiteSpace($script:AccessToken)) {
        $headers["Authorization"] = "Bearer $($script:AccessToken)"
    }

    $request = @{
        Method      = $Method
        Uri         = "$($script:ApiBaseUrl)$Path"
        Headers     = $headers
        ErrorAction = "Stop"
    }

    if ($null -ne $Body) {
        $request["ContentType"] = "application/json"
        $request["Body"] = $Body | ConvertTo-Json -Depth 12 -Compress
    }

    try {
        return Invoke-RestMethod @request
    }
    catch {
        $statusCode = Get-StatusCode $_
        if ($AllowNotFound -and $statusCode -eq 404) {
            return $null
        }

        throw
    }
}

function New-QueryPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path,

        [Parameter(Mandatory)]
        [hashtable]$Parameters
    )

    $pairs = foreach ($entry in $Parameters.GetEnumerator()) {
        if ($null -ne $entry.Value -and -not [string]::IsNullOrWhiteSpace([string]$entry.Value)) {
            "{0}={1}" -f [Uri]::EscapeDataString([string]$entry.Key), [Uri]::EscapeDataString([string]$entry.Value)
        }
    }

    return "${Path}?$($pairs -join '&')"
}

function Find-SingleExactMatch {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [object[]]$Items,

        [Parameter(Mandatory)]
        [string]$PropertyName,

        [Parameter(Mandatory)]
        [string]$ExpectedValue,

        [Parameter(Mandatory)]
        [string]$Label
    )

    $matches = @($Items | Where-Object {
        $value = $_.$PropertyName
        $null -ne $value -and [string]::Equals(
            [string]$value,
            $ExpectedValue,
            [StringComparison]::OrdinalIgnoreCase)
    })

    if ($matches.Count -gt 1) {
        throw "Multiple $Label records matched '$ExpectedValue'. Resolve duplicates before reseeding."
    }

    if ($matches.Count -eq 1) {
        return $matches[0]
    }

    return $null
}

function ConvertTo-NormalizedPhone {
    param([string]$Phone)

    return -join ($Phone.ToCharArray() | Where-Object { [char]::IsDigit($_) })
}

function Ensure-InvitedUser {
    param(
        [string]$Email,
        [string]$FullName
    )

    $users = @(Invoke-MotorCareApi -Method Get -Path "/api/users/")
    $user = Find-SingleExactMatch -Items $users -PropertyName "email" -ExpectedValue $Email -Label "user"

    if ($null -eq $user) {
        Invoke-MotorCareApi -Method Post -Path "/api/users/invite" -Body @{
            email    = $Email
            role     = 4
            fullName = $FullName
        } | Out-Null

        Write-Step "Invited pending technician user $Email."
        return
    }

    if ($user.isEmailVerified) {
        throw "User '$Email' already accepted an invite. Use a fresh invited email or a fresh tenant to restore a pending-invite demo state."
    }

    Write-Step "Pending invited user $Email already exists."
}

function Ensure-Customer {
    param(
        [string]$FullName,
        [string]$Phone,
        [string]$Email,
        [string]$Notes
    )

    $path = New-QueryPath -Path "/api/customers/" -Parameters @{
        q          = $Phone
        pageNumber = 1
        pageSize   = 100
    }
    $result = Invoke-MotorCareApi -Method Get -Path $path
    $normalizedPhone = ConvertTo-NormalizedPhone $Phone
    $matches = @($result.items | Where-Object {
        (ConvertTo-NormalizedPhone ([string]$_.phone)) -eq $normalizedPhone
    })

    if ($matches.Count -gt 1) {
        throw "Multiple customer records matched phone '$Phone'. Resolve duplicates before reseeding."
    }

    $body = @{
        fullName = $FullName
        phone    = $Phone
        email    = $Email
        whatsapp = $Phone
        notes    = $Notes
    }

    if ($matches.Count -eq 0) {
        $id = Invoke-MotorCareApi -Method Post -Path "/api/customers/" -Body $body
        Write-Step "Created customer $FullName."
        return Invoke-MotorCareApi -Method Get -Path "/api/customers/$id"
    }

    $customer = $matches[0]
    Invoke-MotorCareApi -Method Put -Path "/api/customers/$($customer.id)" -Body $body | Out-Null
    Write-Step "Reconciled customer $FullName."
    return Invoke-MotorCareApi -Method Get -Path "/api/customers/$($customer.id)"
}

function Ensure-Vehicle {
    param(
        [string]$Plate,
        [string]$Brand,
        [string]$Model,
        [int]$Year,
        [int]$CurrentKm,
        [object]$Customer
    )

    $vehicle = Invoke-MotorCareApi -Method Get -Path "/api/vehicles/$([Uri]::EscapeDataString($Plate))" -AllowNotFound
    if ($null -eq $vehicle) {
        $id = Invoke-MotorCareApi -Method Post -Path "/api/vehicles/" -Body @{
            plate             = $Plate
            brand             = $Brand
            model             = $Model
            year              = $Year
            currentKm         = $CurrentKm
            currentCustomerId = $Customer.id
        }

        Write-Step "Created vehicle $Plate."
        return Invoke-MotorCareApi -Method Get -Path "/api/vehicles/$([Uri]::EscapeDataString($Plate))"
    }

    if ([string]$vehicle.customerId -ne [string]$Customer.id) {
        throw "Vehicle '$Plate' exists but is assigned to a different customer. The public API has no vehicle reassignment endpoint for safe repair."
    }

    Write-Step "Vehicle $Plate already exists."
    return $vehicle
}

function Ensure-ServiceCatalogItem {
    param(
        [string]$Name,
        [int]$Category,
        [string]$Description,
        [int]$DurationMinutes,
        [decimal]$Price
    )

    $path = New-QueryPath -Path "/api/services/" -Parameters @{
        q          = $Name
        pageNumber = 1
        pageSize   = 100
    }
    $result = Invoke-MotorCareApi -Method Get -Path $path
    $item = Find-SingleExactMatch -Items @($result.items) -PropertyName "name" -ExpectedValue $Name -Label "service catalog"
    $body = @{
        name                   = $Name
        category               = $Category
        description            = $Description
        defaultDurationMinutes = $DurationMinutes
        defaultPrice           = $Price
        price                  = $Price
        currency               = "TRY"
        isActive               = $true
    }

    if ($null -eq $item) {
        $id = Invoke-MotorCareApi -Method Post -Path "/api/services/" -Body $body
        Write-Step "Created service catalog item $Name."
        return Invoke-MotorCareApi -Method Get -Path "/api/services/$id"
    }

    Invoke-MotorCareApi -Method Put -Path "/api/services/$($item.id)" -Body $body | Out-Null
    Write-Step "Reconciled service catalog item $Name."
    return Invoke-MotorCareApi -Method Get -Path "/api/services/$($item.id)"
}

function Ensure-InventoryItem {
    param(
        [string]$Name,
        [string]$Sku,
        [string]$Category,
        [string]$Brand,
        [string]$Unit,
        [decimal]$UnitPrice,
        [decimal]$StockQuantity,
        [decimal]$MinimumStockLevel
    )

    $path = New-QueryPath -Path "/api/inventory/" -Parameters @{
        q          = $Sku
        pageNumber = 1
        pageSize   = 100
    }
    $result = Invoke-MotorCareApi -Method Get -Path $path
    $item = Find-SingleExactMatch -Items @($result.items) -PropertyName "sku" -ExpectedValue $Sku -Label "inventory"
    $body = @{
        name              = $Name
        sku               = $Sku
        barcode           = $null
        category          = $Category
        brand             = $Brand
        unit              = $Unit
        unitPrice         = $UnitPrice
        stockQuantity     = $StockQuantity
        minimumStockLevel = $MinimumStockLevel
        isActive          = $true
    }

    if ($null -eq $item) {
        $id = Invoke-MotorCareApi -Method Post -Path "/api/inventory/" -Body $body
        Write-Step "Created inventory item $Sku."
        return Invoke-MotorCareApi -Method Get -Path "/api/inventory/$id"
    }

    Invoke-MotorCareApi -Method Put -Path "/api/inventory/$($item.id)" -Body $body | Out-Null
    Write-Step "Reconciled inventory item $Sku."
    return Invoke-MotorCareApi -Method Get -Path "/api/inventory/$($item.id)"
}

function Get-ServiceOrderByMarker {
    param([string]$Marker)

    $path = New-QueryPath -Path "/api/service-orders/" -Parameters @{
        q          = $Marker
        pageNumber = 1
        pageSize   = 100
    }
    $result = Invoke-MotorCareApi -Method Get -Path $path
    $matches = @($result.items | Where-Object {
        [string]::Equals([string]$_.complaint, $Marker, [StringComparison]::Ordinal)
    })

    if ($matches.Count -gt 1) {
        throw "Multiple service orders matched marker '$Marker'. Resolve duplicates before reseeding."
    }

    if ($matches.Count -eq 0) {
        return $null
    }

    return Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$($matches[0].id)"
}

function Ensure-OrderOperation {
    param(
        [object]$Order,
        [string]$Description,
        [decimal]$Price
    )

    $existing = @($Order.operations | Where-Object {
        [string]::Equals([string]$_.description, $Description, [StringComparison]::Ordinal)
    })
    if ($existing.Count -eq 0) {
        Invoke-MotorCareApi -Method Post -Path "/api/service-orders/$($Order.id)/operations" -Body @{
            description = $Description
            quantity    = 1
            unitPrice   = $Price
            discount    = 0
            price       = $Price
        } | Out-Null
    }
}

function Ensure-OrderPayment {
    param(
        [object]$Order,
        [decimal]$Amount,
        [int]$Method
    )

    $methodName = switch ($Method) {
        1 { "Cash" }
        2 { "CreditCard" }
        3 { "BankTransfer" }
        default { throw "Unsupported payment method '$Method'." }
    }

    $existing = @($Order.payments | Where-Object {
        [decimal]$_.amount -eq $Amount -and [string]$_.method -eq $methodName
    })
    if ($existing.Count -eq 0) {
        Invoke-MotorCareApi -Method Post -Path "/api/service-orders/$($Order.id)/payments" -Body @{
            amount      = $Amount
            method      = $Method
            paymentDate = $null
        } | Out-Null
    }
}

function Set-ServiceOrderStatus {
    param(
        [object]$Order,
        [int]$Status,
        [string]$Note
    )

    Invoke-MotorCareApi -Method Put -Path "/api/service-orders/$($Order.id)/status" -Body @{
        status = $Status
        note   = $Note
    } | Out-Null
}

function Ensure-ActiveServiceOrder {
    param(
        [object]$Vehicle,
        [object]$Customer
    )

    $marker = "DEMO-ACTIVE: rear brake squeal"
    $order = Get-ServiceOrderByMarker -Marker $marker

    if ($null -eq $order) {
        $id = Invoke-MotorCareApi -Method Post -Path "/api/service-orders/" -Body @{
            vehicleId   = $Vehicle.id
            customerId  = $Customer.id
            vehicleKm   = 18700
            complaint   = $marker
            consumables = @()
        }
        $order = Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$id"
        Write-Step "Created active service order marker."
    }

    if ($order.status -notin @("Open", "InProgress", "WaitingForParts")) {
        throw "Active service order marker exists in terminal status '$($order.status)'. Use a fresh tenant or clean up that demo row before reseeding."
    }

    Ensure-OrderOperation -Order $order -Description "Brake inspection" -Price 300
    $order = Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$($order.id)"

    if ($order.status -eq "Open" -or $order.status -eq "WaitingForParts") {
        Set-ServiceOrderStatus -Order $order -Status 2 -Note "Demo order prepared for active workflow."
        $order = Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$($order.id)"
    }

    Write-Step "Active service order $($order.orderNo) is $($order.status)."
    return $order
}

function Ensure-CompletedServiceOrder {
    param(
        [object]$Vehicle,
        [object]$Customer
    )

    $marker = "DEMO-COMPLETED: 10000 km periodic service"
    $order = Get-ServiceOrderByMarker -Marker $marker

    if ($null -eq $order) {
        $id = Invoke-MotorCareApi -Method Post -Path "/api/service-orders/" -Body @{
            vehicleId  = $Vehicle.id
            customerId = $Customer.id
            vehicleKm  = 9200
            complaint  = $marker
            consumables = @(
                @{
                    category      = "Oil"
                    productName   = "10W-40 engine oil"
                    brand         = "Motul"
                    subCategory   = "Engine oil"
                    specification = "10W-40"
                    quantity      = 3
                    unitPrice     = 425
                    notes         = "Demo consumable"
                }
            )
        }
        $order = Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$id"
        Write-Step "Created completed service order marker."
    }

    if ($order.status -in @("Cancelled", "Delivered")) {
        throw "Completed service order marker exists in terminal status '$($order.status)'. Use a fresh tenant or clean up that demo row before reseeding."
    }

    if ($order.status -ne "Completed") {
        Ensure-OrderOperation -Order $order -Description "Oil change" -Price 850
        $order = Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$($order.id)"
        Ensure-OrderOperation -Order $order -Description "Chain adjustment" -Price 350
        $order = Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$($order.id)"
        Ensure-OrderPayment -Order $order -Amount 1200 -Method 2
        $order = Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$($order.id)"

        if ($order.status -eq "Open") {
            Set-ServiceOrderStatus -Order $order -Status 2 -Note "Demo order started."
            $order = Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$($order.id)"
        }

        Set-ServiceOrderStatus -Order $order -Status 4 -Note "Demo order completed."
        $order = Invoke-MotorCareApi -Method Get -Path "/api/service-orders/$($order.id)"
    }

    $publicAccess = Invoke-MotorCareApi -Method Post -Path "/api/service-orders/$($order.id)/public-access"
    Write-Step "Completed service order $($order.orderNo) is ready."
    return @{
        order        = $order
        publicAccess = $publicAccess
    }
}

function Ensure-InspectionReport {
    param(
        [object]$Vehicle,
        [object]$Customer
    )

    $path = New-QueryPath -Path "/api/inspections/" -Parameters @{
        vehicleId   = $Vehicle.id
        packageType = 4
        pageNumber  = 1
        pageSize    = 100
    }
    $result = Invoke-MotorCareApi -Method Get -Path $path
    $matches = @($result.items | Where-Object {
        [string]$_.vehicleId -eq [string]$Vehicle.id -and [int]$_.packageType -eq 4
    })

    if ($matches.Count -gt 1) {
        throw "Multiple full inspection records exist for vehicle '$($Vehicle.plateOriginal)'. Resolve duplicates before reseeding."
    }

    if ($matches.Count -eq 0) {
        $created = Invoke-MotorCareApi -Method Post -Path "/api/inspections/" -Body @{
            customerId    = $Customer.id
            vehicleId     = $Vehicle.id
            customerName  = $Customer.fullName
            phone         = $Customer.phone
            plate         = $Vehicle.plateOriginal
            brand         = $Vehicle.brand
            model         = $Vehicle.model
            year          = $Vehicle.year
            mileage       = $Vehicle.currentKm
            chassisNumber = $Vehicle.chassisNumber
            engineNumber  = $Vehicle.engineNumber
            query5664     = $null
            mileageQuery  = $null
            packageType   = 4
            generalNotes  = "Demo inspection report."
            testRideNotes = "No abnormality noted during demo test ride."
            cosmeticNotes = "No critical cosmetic findings in demo dataset."
        }
        $inspection = Invoke-MotorCareApi -Method Get -Path "/api/inspections/$($created.id)"
        Write-Step "Created full inspection report marker."
    }
    else {
        $inspection = Invoke-MotorCareApi -Method Get -Path "/api/inspections/$($matches[0].id)"
    }

    if ([int]$inspection.status -eq 4) {
        throw "Inspection '$($inspection.inspectionNo)' is cancelled and cannot be reused for a completed report."
    }

    if ([int]$inspection.status -ne 3) {
        foreach ($item in @($inspection.items)) {
            if ([int]$item.result -ne 1) {
                Invoke-MotorCareApi -Method Put -Path "/api/inspections/$($inspection.id)/items/$($item.id)" -Body @{
                    result = 1
                    notes  = "Demo checked."
                } | Out-Null
            }
        }

        Invoke-MotorCareApi -Method Put -Path "/api/inspections/$($inspection.id)/complete" | Out-Null
        $inspection = Invoke-MotorCareApi -Method Get -Path "/api/inspections/$($inspection.id)"
    }

    $publicAccess = Invoke-MotorCareApi -Method Post -Path "/api/inspections/$($inspection.id)/public-access"
    Write-Step "Inspection report $($inspection.inspectionNo) is ready."
    return @{
        inspection   = $inspection
        publicAccess = $publicAccess
    }
}

if ([string]::IsNullOrWhiteSpace($OwnerPassword)) {
    throw "Set MOTORCARE_DEMO_OWNER_PASSWORD or pass -OwnerPassword. The helper requires a verified existing owner login."
}

Write-Step "Logging in as owner $OwnerEmail for tenant $TenantIdentifier."
$login = Invoke-MotorCareApi -Method Post -Path "/api/auth/login" -Body @{
    tenantIdentifier = $TenantIdentifier
    email            = $OwnerEmail
    password         = $OwnerPassword
}
if ($login.requiresTwoFactor) {
    throw "Owner login requires two-factor verification. Use a demo owner without 2FA for this helper or seed through an interactive flow."
}
$script:AccessToken = $login.accessToken

Ensure-InvitedUser -Email $InvitedUserEmail -FullName "Demo Technician"

$customerOne = Ensure-Customer `
    -FullName "Demo Mehmet Kaya" `
    -Phone "+90 532 900 00 01" `
    -Email "mehmet.demo@garajpass.test" `
    -Notes "Primary active-order demo customer."
$customerTwo = Ensure-Customer `
    -FullName "Demo Fatma Demir" `
    -Phone "+90 532 900 00 02" `
    -Email "fatma.demo@garajpass.test" `
    -Notes "Completed-order and inspection demo customer."

$vehicleOne = Ensure-Vehicle -Plate "34DMO001" -Brand "Honda" -Model "CB500F" -Year 2021 -CurrentKm 18700 -Customer $customerOne
$vehicleTwo = Ensure-Vehicle -Plate "34DMO002" -Brand "Yamaha" -Model "MT-07" -Year 2022 -CurrentKm 9200 -Customer $customerTwo

$serviceItems = @(
    Ensure-ServiceCatalogItem -Name "Oil change" -Category 6 -Description "Engine oil replacement." -DurationMinutes 30 -Price 850
    Ensure-ServiceCatalogItem -Name "Chain adjustment" -Category 6 -Description "Drive-chain tension and lubrication check." -DurationMinutes 20 -Price 350
    Ensure-ServiceCatalogItem -Name "Tire replacement" -Category 5 -Description "Single-wheel tire replacement labor." -DurationMinutes 45 -Price 1200
)

$inventoryItems = @(
    Ensure-InventoryItem -Name "10W-40 engine oil" -Sku "DEMO-OIL-10W40" -Category "Lubricants" -Brand "Motul" -Unit "L" -UnitPrice 425 -StockQuantity 12 -MinimumStockLevel 4
    Ensure-InventoryItem -Name "Oil filter" -Sku "DEMO-OIL-FILTER" -Category "Filters" -Brand "Hiflo" -Unit "Piece" -UnitPrice 280 -StockQuantity 10 -MinimumStockLevel 3
    Ensure-InventoryItem -Name "Brake pad set" -Sku "DEMO-BRAKE-PAD" -Category "Brakes" -Brand "Brembo" -Unit "Set" -UnitPrice 1450 -StockQuantity 6 -MinimumStockLevel 2
)

$activeOrder = Ensure-ActiveServiceOrder -Vehicle $vehicleOne -Customer $customerOne
$completedOrderResult = Ensure-CompletedServiceOrder -Vehicle $vehicleTwo -Customer $customerTwo
$inspectionResult = Ensure-InspectionReport -Vehicle $vehicleTwo -Customer $customerTwo

[pscustomobject]@{
    tenantIdentifier                = $TenantIdentifier
    ownerEmail                      = $OwnerEmail
    invitedUserEmail                = $InvitedUserEmail
    customerIds                     = @($customerOne.id, $customerTwo.id)
    vehicleIds                      = @($vehicleOne.id, $vehicleTwo.id)
    serviceCatalogItemIds           = @($serviceItems | ForEach-Object { $_.id })
    inventoryItemIds                = @($inventoryItems | ForEach-Object { $_.id })
    activeServiceOrderId            = $activeOrder.id
    completedServiceOrderId         = $completedOrderResult.order.id
    completedServiceOrderPublicSlug = $completedOrderResult.publicAccess.slug
    inspectionId                    = $inspectionResult.inspection.id
    inspectionPublicSlug            = $inspectionResult.publicAccess.slug
} | ConvertTo-Json -Depth 6
