# Welcome to the SQL Server Developer Deployment project
This project will set up resources that allow you to easily deploy Microsoft SQL Server Developer edition across 
EC2 instances in your account automatically. 

# Prerequitits
You must meet the follwoing pre-requisis in order to use this progect:
* Have the AWS Command Line installed
* Have the AWSPowerShell Commandlet installed on your machine
* Have a default profile configured with administrative access to the AWS Account you want to deploy to.

# Instalation
## Step 1
Download the SQL Server Developer install from Microsoft.
https://go.microsoft.com/fwlink/?linkid=866662

## Step 2
Run the SQL Server Developer install, and select the option to "Download Media" this will download the SQL Server Developer install Media (ISO) to your computer.

## Step 3 
Copy the SQL Server Developer ISO to the s3DeploymentFiles/ directory in this project.

## Step 4
Open Powershell and change directory to the project directory.
Run the following Powershell command
`Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`
Run the DeployToAWS.ps1 powershell script to deploy this project to your account.

This script will create resources in your AWS Account including:
* S3 bucket to hold your install media
* IAM Role with permissions to access SSM and and the S3 bucket above
* IAM Instance profile to attach the role above to your EC2 instances
* Parameter Store parameters to control the instalation
* SSM Command document to perform the deployment

# Using the project
Once you have deployed the project, you are able to use the created resources to deploy SQL Server Developer to instances using the Command Document created.

## Parameters
The follwoing parameters are create in the SSM Parameter Store and referanced in the deployment:
* SQL.Developer.InstallBucket - The S3 Bucket where the install files have been copied.
* SQL.Developer.DefaultAdmin - The default Admin user for the SQL Server install. 
* SQL.Developer.iso - The name of the SQL Server ISO that should be used.

You should update these settings as needed for your install of SQL Server Developer. If your instances are domain joined, you may want to set the Default Admin account to be a domain user instead of the local admin account. 

The ISO parameter should be changed if you use an updated media file for SQL Server developer.

## EC2 Instance
The EC2 instance that you want to deploy to must have the role "sql-developer-deployment-role" attached. This role will be created as part of the project deployent. This role has permissions to communicate with SSM, and has rad permissions to the bucket that was created as part of the deployment. 

The EC2 instace is assumed not to have any SQL Server tools indstalled on it. 

# Command Document
A Command Document is automatically created under SSM Documents. This document is called "Install-SQL-Developer" and can be found under SSM Documents, by selecting the "Owned By Me" tag. You can run this document against individual instances, or select instances based on tags. 

This document will automatically pull the SQL Serevr install media from the S3 bucket configured at deployment time, as well as the configuration script. It will then execute a silent install of SQL Server Developer on the machine. The provided configuration will install SQL Server developer on the C drive of the instance, and enable TCP connections to the SQL Server.

Note: The default security groups assigned to a Windows instance will NOT allow connectivity for SQL Server. You will need to manually enable this in the security groups for your instaces before you can connect to them. The automation does not make any changes to the security groups deployed on your instances.

The install document will NOT install the SQL Server Management tools on your instance. 


# Customization
## Customize the template:
The main CloudFormation template is stored in the StackDeploymentFiles sub directory for this project. This is template is developed using AWS CDK. You can update the stack by editing the CDK project and then updating the stack using the CDK command
`cdk synth > StackDeploymentFiles\SQLDeveloperStack.yml`

In order to update the stack using CDK you must first have the CDK Command Line installed. 
You can find details on getting started with CDK here:
https://docs.aws.amazon.com/cdk/latest/guide/getting_started.html

## Customize the SQL Instalation
This deployment uses the file s2DeploymentFiles/ConfigurationFile.ini to control the configuration of the SQL Instalation. You can update this file to change the way that SQL Server Developer is installed. 