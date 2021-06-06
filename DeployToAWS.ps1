# Run this to let the script run as it is not signed.
# Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# Import the AWS Powershell Commandlet
Import-Module AWSPowerShell

#
# Deploy the stack that sets up most of the AWS Resources for this project.
#
$template = Get-Content -Path .\StackDeploymentFiles\SQLDeveloperStack.yml -Raw
New-CFNStack -StackName "AwsSQLDeveloper-Deployment" -Capability CAPABILITY_NAMED_IAM -TemplateBody $template -TimeoutInMinutes 10
Wait-CFNStack -StackName "AwsSQLDeveloper-Deployment" -Timeout 600



#
# Copy the configuration file and the ISO file to S3 to be used later.
#
$isofile = "SQLServer2019-x64-ENU-Dev.iso"
$configFile = "ConfigurationFile.ini"
$ssmsfile = "SSMS-Setup-ENU.exe"

$S3MediaBucket = (Get-SSMParameter -Name SQL.Developer.InstallBucket).Value
Write-S3Object -File s3DeploymentFiles\$isofile -Bucket $S3MediaBucket -Key $isofile
Write-S3Object -File s3DeploymentFiles\$configFile -Bucket $S3MediaBucket -Key $configFile

# Check if the user has downloaded SSMS and copy it if it exists.
# Then update the SSM parameter so that the deployment script knows to deploy SSMS.
if ((Test-Path -Path s3DeploymentFiles\$ssmsfile -PathType Leaf)) {
  Write-S3Object -File s3DeploymentFiles\$ssmsfile -Bucket $S3MediaBucket -Key $ssmsfile
  Write-SSMParameter -Name "SQL.Developer.SSMS" -Value $ssmsfile -Overwrite $TRUE
}


#
# Create the Deployment command file to be used for deploying SQL Server Developer to your instances.
#
$document = Get-Content -Path .\InstallSqlDeveloper.json | Out-String
New-SSMDocument -Name "Install-SQL-Developer" -DocumentType Command -Content $document
