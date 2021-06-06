using Amazon.CDK;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using System.Text;

namespace AwsSqlDeveloper
{
    public class AwsSqlDeveloperStack : Stack
    {
        internal AwsSqlDeveloperStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var bucket = new Bucket(this, "sql-developer-deployment-files", new BucketProps
            {
                
            });

            var role = new Role(this, "sql-developer-deployment-role", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
            });
            role.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Resources = new[] 
                {
                    bucket.BucketArn,
                    bucket.ArnForObjects ("*")
                },
                Actions = new[] { "s3:GetObject" }
            }));
            role.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("AmazonSSMManagedInstanceCore"));

            var instanceProfile = new CfnInstanceProfile(this, "sql-developer-deployment-profile", new CfnInstanceProfileProps()
            {
                InstanceProfileName = "sql-developer-deployment-role",
                Roles = new string[] { role.RoleName }
            });

            var bucketParm = new StringParameter(this, "SQL.Developer.InstallBucket", new StringParameterProps()
            {
                ParameterName = "SQL.Developer.InstallBucket",
                StringValue = bucket.BucketName
            });
            bucketParm.GrantRead(role);

            var userParm = new StringParameter(this, "SQL.Developer.DefaultAdmin", new StringParameterProps()
            {
                ParameterName = "SQL.Developer.DefaultAdmin",
                StringValue = @".\administrator"
            });
            userParm.GrantRead(role);

            var studioParm = new StringParameter(this, "SQL.Developer.SSMS", new StringParameterProps()
            {
                ParameterName = "SQL.Developer.SSMS",
                StringValue = @"none"
            });
            studioParm.GrantRead(role);

            var isoParm = new StringParameter(this, "SQL.Developer.iso", new StringParameterProps()
            {
                ParameterName = "SQL.Developer.iso",
                StringValue = @"SQLServer2019-x64-ENU-Dev.iso"
            });
            userParm.GrantRead(role);

            new CfnOutput(this, "bucketName", new CfnOutputProps 
            {
                Value = bucket.BucketName,
                Description = "Bucket to put the install media in.",
                ExportName = "Media-Bucket"
            });
            new CfnOutput(this, "roleName", new CfnOutputProps
            {
                Value = role.RoleName,
                Description = "Assign this role to the instance in order to install SQL Server Developer",
                ExportName = "Instance-Role-Name"
            });

        }
    }
}
