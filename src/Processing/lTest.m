M = csvread('lTest.csv');
f = figure()
p = uipanel('Parent',f,'BorderType','none'); 
p.Title = 'Linearity Test'; 
p.TitlePosition = 'centertop'; 
p.FontSize = 16;
p.FontWeight = 'bold';

MAX = 60000;

for j = 1:1:4
    
    I = (M(:,6) == j);

    xAx = M(I,1)/MAX *100;
    yAx = M(I,2);
    
    subplot(2,2,1,'Parent',p)
    plot(xAx, yAx, '.','MarkerSize',12);
    ylim([0, .35]);
    xlabel('PWM (%)')
    ylabel('Output Voltage (V)')
    title('Sensor 1')
    hold on
    
    if j == 1 || j == 3
        COLOR = 'RED';
    else
        COLOR = 'IR';
    end
    
    mdl = fitlm(xAx,yAx);
       
    legendInfo{j} = ['LED - ' num2str(j) ' (' COLOR ') R^2 = ' num2str(mdl.Rsquared.Ordinary)]; % or whatever is appropriate
end
legend(legendInfo, 'Location','NorthWest');

for j = 1:1:4 
    
    I = (M(:,6) == j);

    xAx = M(I,1)/MAX *100;
    yAx = M(I,3);

    subplot(2,2,2,'Parent',p)
    plot(xAx, yAx, '.','MarkerSize',12);
    ylim([0, .35])
    xlabel('PWM (%)')
    ylabel('Output Voltage (V)')
    title('Sensor 2')
    hold on
end
legend(legendInfo, 'Location','NorthWest');

for j = 1:1:4 
    
    I = (M(:,6) == j);

    xAx = M(I,1)/MAX *100;
    yAx = M(I,4);

    subplot(2,2,3,'Parent',p)
    plot(xAx, yAx, '.','MarkerSize',12);
    ylim([0, .35])
    xlabel('PWM (%)')
    ylabel('Output Voltage (V)')
    title('Sensor 3')
    hold on
end
legend(legendInfo, 'Location','NorthWest');

for j = 1:1:4 
    
    I = (M(:,6) == j);

    xAx = M(I,1)/MAX *100;
    yAx = M(I,5);

    subplot(2,2,4,'Parent',p)
    plot(xAx, yAx, '.','MarkerSize',12);
    ylim([0, .35])
    xlabel('PWM (%)')
    ylabel('Output Voltage (V)')
    title('Sensor 4')
    hold on
end
legend(legendInfo, 'Location','NorthWest');


